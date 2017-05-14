pipeline {
  agent {
    docker {
      image 'c7-jenkins-mono4'
    }
    
  }
  stages {
    stage('Build') {
      steps {
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