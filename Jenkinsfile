pipeline {
  options{
    parameters{[
      string(name: 'NUGET_PUBLISH', defaultValue: 'false', description: 'If this parameter is set, the build will try to publish the artifact to NuGet.', ),
      string(name: 'NUGET_API_KEY', defaultValue: '', description: 'Head over to http://nuget.org/ and register for an account. Once you do that, click on "My Account" to see an API Key that was generated for you.', ),
      string(name: 'DOTNET_CONFIG', defaultValue: 'Debug', choices: ['Debug', 'Release'], description: 'Debug will produce symbols in the assmbly to be able to debug it at runtime. This is the recommended option for feature, hotfix testing or release candidate.<br/><strong>For publishing a release from master branch, please choose Release.</strong>', ),
     ]}
  }
  agent {
    docker {
      image 'docker.terradue.com/c7-jenkins-mono4'
    }
    
  }
  stages {
    stage('Build') {
      steps {
        echo "${DOTNET_CONFIG}"
        sh '''ls -la
rm -rf packges */bin build
nuget restore
mkdir build'''
        sh 'xbuild /p:Configuration=${DOTNET_CONFIG}'
      }
    }
    stage('Package') {
      steps {
        parallel(
          "Package": {
            sh '''nuget4mono -g ${GIT_BRANCH} -p Terradue.OpenSearch/packages.config Terradue.OpenSearch/bin/Terradue.OpenSearch.dll
cat *.nuspec
nuget pack -OutputDirectory build
echo ${NUGET_PUBLISH}'''
            
          },
          "Test": {
            sh 'nunit-console4 *.Test/bin/*.Test.dll -xml build/TestResult.xml'
            nunit(testResultsPattern: 'build/TestResult.xml')
            
          }
        )
      }
    }
    stage('Publish') {
      steps {
        waitUntil() {
          sh '''nuget push build/*.nupkg -ApiKey ${NUGET_API_KEY} -Source https://nuget.org/api/v2/package
'''
        }
        
      }
    }
  }
}