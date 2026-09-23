
pipeline {
    agent any

    environment {
        API_PROJECT           = "LovEat.API.csproj"
        TEST_PROJECT          = "API.Tests/LovEat.API.Tests.csproj"
        DOCKERHUB_REPO        = "yourdockerhubusername/loveat-api"
        IMAGE_TAG             = "dev-${env.BUILD_NUMBER}"
        SONAR_PROJECT_KEY     = "loveat-api"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        stage('Restore') {
            steps {
                sh 'dotnet restore "$API_PROJECT" "$TEST_PROJECT"'
            }
        }
        stage('Build') {
            steps {
                sh 'dotnet build "$API_PROJECT" --no-restore --configuration Release'
            }
        }
        stage('Unit Tests') {
            steps {
                sh 'dotnet test "$TEST_PROJECT" --no-restore --configuration Release --logger "trx;LogFileName=test-results.trx" --collect:"XPlat Code Coverage"'
            }
            post {
                always {
                    junit '**/TestResults/*.trx'
                }
            }
        }
        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('SonarQubeLocal') {
                    sh '''
                        dotnet tool install --global dotnet-sonarscanner || true
                        export PATH="$PATH:$HOME/.dotnet/tools"
                        dotnet sonarscanner begin /k:"$SONAR_PROJECT_KEY" /d:sonar.host.url="$SONAR_HOST_URL" /d:sonar.token="$SONAR_AUTH_TOKEN"
                        dotnet build "$API_PROJECT" --configuration Release
                        dotnet sonarscanner end /d:sonar.token="$SONAR_AUTH_TOKEN"
                    '''
                }
            }
        }
        stage('Quality Gate') {
            steps {
                timeout(time: 5, unit: 'MINUTES') {
                    waitForQualityGate abortPipeline: true
                }
            }
        }
        stage('Docker Build') {
            steps {
                sh 'docker build -t $DOCKERHUB_REPO:$IMAGE_TAG -f Dockerfile .'
            }
        }
        stage('Trivy Scan') {
            steps {
                sh 'trivy image --severity HIGH,CRITICAL --exit-code 1 --ignore-unfixed $DOCKERHUB_REPO:$IMAGE_TAG'
            }
        }
        stage('Push to DockerHub') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'dockerhub-creds', usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                    sh '''
                        echo "$DOCKER_PASS" | docker login -u "$DOCKER_USER" --password-stdin
                        docker push $DOCKERHUB_REPO:$IMAGE_TAG
                        docker tag $DOCKERHUB_REPO:$IMAGE_TAG $DOCKERHUB_REPO:dev-latest
                        docker push $DOCKERHUB_REPO:dev-latest
                    '''
                }
            }
        }
        stage('Deploy (local DEV via Docker Compose)') {
            steps {
                withCredentials([string(credentialsId: 'sa-password', variable: 'SA_PASSWORD')]) {
                    sh '''
                        docker compose down --remove-orphans || true
                        docker compose up --build -d
                    '''
                }
            }
        }
        stage('Smoke Test') {
            steps {
                sh '''
                    sleep 20
                    curl -f http://localhost:8081/health
                '''
            }
        }
    }

    post {
        success {
            echo "DEV pipeline succeeded"
        }
        failure {
            echo "Pipeline failed - check stage logs above."
        }
        always {
            sh 'docker compose logs --tail 50 || true'
        }
    }
}
| Set-Content -Path .\Jenkinsfile -Encoding utf8NoBOM