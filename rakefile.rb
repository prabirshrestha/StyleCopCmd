require File.join(File.dirname(__FILE__), 'libs/albacore/albacore.rb')

BASE_VERSION = '1.2.3'

CONFIGURATION   =   :Release

ROOT_DIR				= File.dirname(__FILE__) + "/"
SRC_PATH				= ROOT_DIR + "src/"
LIBS_PATH				= ROOT_DIR + "libs/"
OUTPUT_PATH				= ROOT_DIR + "bin/"
DIST_PATH				= ROOT_DIR + "dist/"

CI_BUILD_NUMBER_PARAM_NAME = 'BUILD_NUMBER'

begin
	gitcommit = `git log -1 --pretty=format:%H`
rescue
	gitcommit = "nogit"
end

NIGHTLY = ENV['NIGHTLY'].nil? ? 'true' : ENV['NIGHTLY'].downcase

CI_BUILD_NUMBER = ENV[CI_BUILD_NUMBER_PARAM_NAME] || 0

if ENV[CI_BUILD_NUMBER_PARAM_NAME] == nil || NIGHTLY == 'true' then
	# if we are not running under teamcity or someother CI like hudson.
	# or if nightly is true.
	# generate the version number based on VERSION file.
	VERSION_NO = "#{BASE_VERSION}.#{CI_BUILD_NUMBER}"
else
	# if we are running inside teamcity, then it passes the full version
	# so ignore the VERSION file and overwrite the VERSION_NO and VERSION_LONG
	VERSION_NO = ENV['BUILD_NUMBER']
end

if NIGHTLY == 'true' then
	VERSION_LONG = "#{VERSION_NO}-nightly-#{gitcommit[0..5]}" 
else
	VERSION_LONG = "#{VERSION_NO}-#{gitcommit[0..5]}" 
end

puts
puts "Base Version: #{BASE_VERSION}"
puts "Version Number: #{VERSION_NO}   :  #{VERSION_LONG} "
print "CI Build Number: "
print CI_BUILD_NUMBER
print " (not running under CI mode)" if CI_BUILD_NUMBER == 0
puts
puts "Git Commit Hash: #{gitcommit}"
puts

task :default => :full
task :full => [:build_release,:package_binaries]

desc "Prepare build"
task :prepare => [:clean] do
    mkdir OUTPUT_PATH unless File.exists?(OUTPUT_PATH)
	mkdir DIST_PATH unless File.exists?(DIST_PATH)
    cp "LICENSE.txt", OUTPUT_PATH
	cp "README.md" , OUTPUT_PATH
end

desc "Clean"
task :clean => [:clean_msbuild] do
    FileUtils.rm_rf OUTPUT_PATH
	FileUtils.rm_rf DIST_PATH
end

desc "Clean msbuild outputs"
msbuild :clean_msbuild do |msb|
    msb.properties :configuration => CONFIGURATION
    msb.solution = "#{SRC_PATH}StyleCop.sln"
    msb.targets :Clean
end

desc "Build solution (default)"
msbuild :build_release => [:prepare] do |msb|
	msb.properties :configuration => CONFIGURATION
	msb.solution = "#{SRC_PATH}StyleCop.sln"
	msb.targets	:Build
end

desc "Create a zip package for the release binaries"
zip :package_binaries => [:build_release] do |zip|
	zip.directories_to_zip OUTPUT_PATH
    zip.output_file = "StyleCopCmd-#{VERSION_LONG}-bin.zip"
    zip.output_path = DIST_PATH
end

desc "Create a source package (requires git in PATH)"
task :package_source do
	mkdir DIST_PATH unless File.exists?(DIST_PATH)
	sh "git archive HEAD --format=zip > dist/StyleCopCmd-#{VERSION_LONG}-src.zip"
end