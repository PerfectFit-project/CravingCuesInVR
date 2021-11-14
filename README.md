# Craving Cues in VR

## Networked Solution
### Goals
1. Develop a system that takes textures and audio from the streamingassets folder at runtime and presents them as a virtual environment.
2. Develop UI to facilitate communication between researchers and participants during the experiment.

### Progress Status
1. Researcher and participant UI complete in terms of planned functionality. 
2. Implemented a panoramic photo viewer using a camera attached to a sphere and rendering the desired texture inside it.
3. Implemented networking: 
	1. Host (researcher) and client (participant) can communicate via the former selecting a message template (loaded from a JSON file) with acceptable responses, and the latter selecting the response desired.
	2. The client (participant) can look around in the environments using an HMD, while the host (researcher) can only view what the participant is viewing.

### (Rough) Plan
1. Perform final tests.

### Use
Open as Unity project, open the "PanoramicEnvsNetworkingVR" Scene, build the project for PC Standalone with Windows as the target platform, and run. 
Need two instances running to test the communication functionality, i.e. editor and an instance from the build executable, or two instances of the built executable, one being the researcher (host) and the other being the participant (client). Host password is "a". Sets up a server and connects on the localhost network address.
Camera movement only works with HDM in this version, but if using the editor as one running instance, the camera can be manually rotated through the object in the hierarchy.
By default the UI is not visible. In both Researcher and UI instances, the key combination Left-Control + Space will open / close the UI. Researcher UI interaction works with a mouse. Participant UI also works with a gamepad controller (XBOX One controller confirmed).
Need to specify which environment to load in the EnvironmentToPresent.json file. The JSON file specifies the bar scene by default. 

-------

## Local Experiment
### Goals
1. Develop a system that takes textures and audio from the streamingassets folder at runtime and presents them as virtual environment based on the order defined in the relevant JSON file.
2. Develop UI to facilitate obtaining responses to questionnaires entered defined in the relevant JSON file.

### Progress Status
1. Implemented a panoramic photo viewer using a camera attached to a sphere and rendering the desired texture inside it.
2. Implemented UI that presents questionnaires.
3. Implemented recording questionnaire responses and exporting them in a JSON file.
4. Implemented FSM to handle running the experiment.

### (Rough) Plan
1. Perform final tests.

### Use
Open as Unity project, open the "PanoramicEnvsNetworkingVR" Scene, build the project for PC Standalone with Windows as the target platform, and run. 
Currently each environment is presented for 180 seconds, and transitional environments for 35 seconds. This can be changed through the PlayerLC object via the Unity editor.
Camera movement is controlled via an HMD, and UI navigation using a gamepad controller (XBOX One controller confirmed). UI interaction also works with a mouse.
