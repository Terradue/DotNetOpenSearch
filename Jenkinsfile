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
        sh 'ls -la'
      }
    }
    stage('Build') {
      steps {
        echo "The library will be build in ${params.DOTNET_CONFIG}"
        sh "msbuild /t:build /p:Configuration=${params.DOTNET_CONFIG} /restore:True"
      }
    }
    stage('Package') {
      steps {
        sh "msbuild /t:pack /p:Configuration=${params.DOTNET_CONFIG}"
        sh 'cat */obj/*/*.nuspec'
      }
    }
    stage('Test') {
      steps {
        sh 'mono packages/xunit.runner.console/2.4.1/tools/net452/xunit.console.exe -junit TestResult.xml Terradue.OpenSearch.Test/bin/Debug/net4*/*.Test.dll'
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
        sh "nuget push build/*.nupkg -ApiKey ${params.NUGET_API_KEY} -Source https://www.nuget.org/api/v2/package"
      }       
    }
  }
  post { 
    always { 
       nunit(testResultsPattern: 'TestResult.xml')
    }
  }
}