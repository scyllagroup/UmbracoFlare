module.exports = function(grunt) {
  require('load-grunt-tasks')(grunt);
  var path = require('path')

  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),
    pkgMeta: grunt.file.readJSON('config/meta.json'),
    dest: grunt.option('target') || 'dist',
    basePath: path.join('<%= dest %>', 'App_Plugins', '<%= pkgMeta.name %>'),

    watch: {
      options: {
        spawn: false,
        atBegin: true
      },


      app_plugins: {
        files: ['UmbracoFlare/**/**'],
        tasks: ['copy:app_plugins']
      },

      dll: {
        files: ['UmbracoFlare/**/*.cs'],
        tasks: ['msbuild:dist', 'copy:dll']
      },

      config: {
	files: ['Umbracoflare/Config/cloudflare.config'],
	tasks: ['copy:config']
      },
      transforms:{
      }
    },

    copy: {
      app_plugins: {
        cwd: 'UmbracoFlare/App_Plugins/UmbracoFlare',
        src: ['**'],
        dest: '<%= basePath %>',
        expand: true
      },
      config: {
	cwd: 'UmbracoFlare/Config/',
        src: 'cloudflare.config',
	dest: '<%= dest %>/config/',
	expand: true
      },
      transforms:{
      },
      dll: {
        cwd: 'UmbracoFlare/bin/Release/',
        src: 'UmbracoFlare.dll',
        dest: '<%= dest %>/bin/',
        expand: true
      },

      nuget: {
        files: [
          {
            cwd: '<%= dest %>/App_Plugins',
            src: ['**/*', '!bin', '!bin/*'],
            dest: 'tmp/nuget/content/App_Plugins',
            expand: true
          },
          {
            cwd: '<%= dest %>/Config',
            src: ['**/*', '!bin', '!bin/*'],
            dest: 'tmp/nuget/content/Config',
            expand: true
          },
	  {
            cwd: '<%= dest %>/transforms',
            src: ['**/*', '!bin', '!bin/*'],
            dest: 'tmp/nuget/content/Config',
            expand: true
          },          
          {
            cwd: '<%= dest %>/UmbracoFlare/',
            src: ['**/*'],
            dest: 'tmp/nuget/content/UmbracoFlare',
            expand: true
          },
          {
            cwd: '<%= dest %>/bin',
            src: ['*.dll'],
            dest: 'tmp/nuget/lib/net40',
            expand: true
          }
        ]
      },
      umbraco: {
        cwd: '<%= dest %>',
        src: '**/*',
        dest: 'tmp/umbraco',
        expand: true
      }
    },

    nugetpack: {
        dist: {
            src: 'tmp/nuget/package.nuspec',
            dest: 'pkg'
        }
    },

    template: {
        'nuspec': {
            'options': {
                'data': { 
                    name: '<%= pkgMeta.name %>',
                    version: '<%= pkgMeta.version %>',
                    url: '<%= pkgMeta.url %>',
                    license: '<%= pkgMeta.license %>',
                    licenseUrl: '<%= pkgMeta.licenseUrl %>',
                    author: '<%= pkgMeta.author %>',
                    authorUrl: '<%= pkgMeta.authorUrl %>',
                    files: [{ path: 'tmp/nuget/content/App_Plugins', target: 'content/App_Plugins'}]
                }
            },
            'files': { 
                'tmp/nuget/package.nuspec': ['config/package.nuspec']
            }
        }
    },

    umbracoPackage: {
      options: {
        name: "<%= pkgMeta.name %>",
        version: '<%= pkgMeta.version %>',
        url: '<%= pkgMeta.url %>',
        license: '<%= pkgMeta.license %>',
        licenseUrl: '<%= pkgMeta.licenseUrl %>',
        author: '<%= pkgMeta.author %>',
        authorUrl: '<%= pkgMeta.authorUrl %>',
        manifest: 'config/package.xml',
        readme: 'config/readme.txt',
        sourceDir: 'tmp/umbraco',
        outputDir: 'pkg',
      }
    },

    clean: {
      build: '<%= grunt.config("basePath").substring(0, 4) == "dist" ? "dist/**/*" : "null" %>',
      tmp: ['tmp']
    },

    assemblyinfo: {
      options: {
        files: ['UmbracoFlare/UmbracoFlare.csproj'],
        filename: 'AssemblyInfo.cs',
        info: {
          version: '<%= (pkgMeta.version.indexOf("-") ? pkgMeta.version.substring(0, pkgMeta.version.indexOf("-")) : pkgMeta.version) %>', 
          fileVersion: '<%= pkgMeta.version %>'
        }
      }
    },

    msbuild: {
      options: {
        stdout: true,
        verbosity: 'quiet',
        maxCpuCount: 4,
        version: 4.0,
        buildParameters: {
          WarningLevel: 2,
          NoWarn: 1607
        }
      },
      dist: {
        src: ['UmbracoFlare/UmbracoFlare.csproj'],
        options: {
          projectConfiguration: 'Debug',
          targets: ['Clean', 'Rebuild'],
        }
      }
    }
  });

  grunt.registerTask('default', ['clean', 'assemblyinfo', 'msbuild:dist', 'copy:dll', 'copy:app_plugins', 'copy:config', 'copy:transforms']);
  grunt.registerTask('nuget',   ['clean:tmp', 'default', 'copy:nuget', 'template:nuspec', 'nugetpack']);
  grunt.registerTask('umbraco', ['clean:tmp', 'default', 'copy:umbraco', 'umbracoPackage']);
  grunt.registerTask('package', ['clean:tmp', 'default', 'copy:nuget', 'template:nuspec', 'nugetpack', 'copy:umbraco', 'umbracoPackage', 'clean:tmp']);
};