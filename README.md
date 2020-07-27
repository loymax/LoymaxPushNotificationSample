# PushApp - Test application for Push Notification system using Loymax PublicAPI

## Setup

Application does not contain *Google-Services.json*, you should to create your own:
- Change application Package ID inside AndroidManifest file from "com.test.pushapp" to yours one
- Complete registration on [Firebase Console](https://console.firebase.google.com/) and create your project there
- Go to "Project settings -> General" and add your application Package ID there 
- Download "google-services.json" file and add it into project
- Change its "Build Action" from "None" to "GoogleServicesJson"
- Build and run the project

More detailed information about Firebase SDK setup you could find on [official firebase documentation page](https://firebase.google.com/docs/cloud-messaging/android/client).