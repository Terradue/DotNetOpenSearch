pipeline {
  parameters{
    booleanParam(name: 'NUGET_PUBLISH', defaultValue: false, description: 'If this parameter is set, the build will try to publish the artifact to NuGet.', )
    string(name: 'NUGET_API_KEY', defaultValue: '', description: 'Head over to http://nuget.org/ and register for an account. Once you do that, click on "My Account" to see an API Key that was generated for you.', )
    choice(name: 'DOTNET_CONFIG', choices: "Debug\nRelease", description: 'Debug will produce symbols in the assmbly to be able to debug it at runtime. This is the recommended option for feature, hotfix testing or release candidate.<br/><strong>For publishing a release from master branch, please choose Release.</strong>', )
   }
  agent { node { label 'centos7-mono4' } }
  stages {
    stage('Init') {
      steps {
        sh 'rm -rf packges */bin build'
        sh 'mkdir -p build'
        sh 'nuget restore'
        sh 'ls -la'
      }
    }
    stage('Build') {
      steps {
        echo "The library will be build in ${params.DOTNET_CONFIG}"
        sh "xbuild /p:Configuration=${params.DOTNET_CONFIG}"
      }
    }
    stage('Package') {
      steps {
        parallel(
          "Package": {
            sh "nuget4mono -g ${env.GIT_BRANCH} -p Terradue.OpenSearch/packages.config Terradue.OpenSearch/bin/Terradue.OpenSearch.dll"
            sh 'cat *.nuspec'
            sh 'nuget pack -OutputDirectory build'
            sh "echo ${params.NUGET_PUBLISH}"
            
          },
          "Test": {
            sh 'nunit-console4 *.Test/bin/*.Test.dll -xml build/TestResult.xml'
          }
        )
      }
    }
    stage('Publish') {
      when {
        expression {
          return params.NUGET_PUBLISH
        }
      }
      steps {
        echo 'Deploying'
        sh 'nuget push build/*.nupkg -ApiKey ${params.NUGET_API_KEY} -Source https://nuget.org/api/v2/package'
      }       
    }
        
  }
  post { 
    always { 
       nunit(testResultsPattern: 'build/TestResult.xml')
    }
  }
}