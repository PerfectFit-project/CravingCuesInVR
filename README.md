# Craving Cues in VR
Project to present environments in VR and enable different functionality depending on which scene is used to build an executable. Networked solution allows for communication between two instances of the application, local experiment enables conducting an experiment to collect participant responses after viewing different virtual environments, and the Syste-use tutorial acts as a short exposition and exercise for participants to familiarize themselves with VR.

Additional material can be found in the *Accompanying Material* folder:
1. *Documents* subfolder:
	1. *Technical Design Document.pdf*: Describes objects and their attached scripts in the Unity project.
	2. *Researcher Guidelines Document.pdf*r: Provides guidelines for:
		1. Preparing audiovisual material.
		2. Editing .json files to specify which virtual environments to present, what messages to offer researchers to send, and what questionnaire to present participants.
		3. How to use the system.
	3. *System Architecture.png*: Diagram ilustrating the system architecture
2. *Non-Personalized Virtual Environment Material* subfolder: 
	1. Contains photos and audio incorporated by our system to create the non-personalized virtual environments used in our experiment, in a ready-to-use form. *NOPNSC_X* refers to "non-personalized non-smoking environment X", and *NOPSC_X* refers to "non-personalized smoking environment X".
	2. *Non-Vertically-Stretched Photos* subfolder containing the non-personalized environment photos in the stage before stretching them to be used by our system. *NS - X* refers to "non-smoking environment X", and *S - X* refers to "smoking environment X".

<br/>

-------

<br/>

## Networked Solution
### Description
1. System that takes textures and audio from the Streamingassets folder at runtime and presents them as a virtual environment.
2. Establishes network commection (via local host or Unity Relay server) to facilitate communication between researchers and participants during the experiment, and for updating the Researcher camera based on Participant camera rotation.
3. Presents UI based on user type:
	1. Researcher: UI that allows for selecting message from template or composing one from scratch, along with their predefined responses, and sending it to the Participant. Presents the chat history. Facilitates sending a request for the Participant to submit responses to a questionnaire.
	2. Participant: UI rendered on a 3D virtual smartphone that allows for the viewing of messages sent by Researchers, selecting the desired response if applicable, and for submitting responses to a questionnaire.



### Use
Open as Unity project, open the *PanoramicEnvsNetworkingVR* Scene, build the project for PC Standalone with Windows as the target platform, and run. Not tested on other platforms, but may also work as intended. Likely incompatibilities include folder paths and file loading/saving, and functionalities related to the VR headset. 
Need two instances running to test the communication functionality. Either editor and an instance from the build executable, or two instances of the built executable. One instance needs to log in as Researcher (host) and the other as Participant (client). Host password is **a**. 

Either (via a ticking the relevant checkbox on the UI): 
1. Host sets up a server and connects on the localhost network address. Waits for client to connect through the localhost network address as well.
2. Host sets up Unity Relay server, which produces a Relay ID key. This key needs to be passed to the participant who has to enter it at login, so that they can connect to the server created by the host. 

Camera movement only works with HDM in this version, but if using the editor as one running instance, the camera can be manually rotated by adjusting the Transform Rotation of the "Camera Offset" object attached to the "XR Rig" object in the hierarchy.

By default the UI is not visible. In both Researcher and UI instances, the key combination Left-Control + Space will toggle the UI. Researcher UI interaction works with a mouse. Participant UI also works with a gamepad controller (XBOX One controller confirmed). The gamepad button to toggle the Participant UI is "Y".

Need to specify which environment to load in the *EnvironmentToPresent.json* file. The JSON file specifies the bus stop scene by default. The naming scheme is as follows: environmentname\_image.png, environmentname\_audio.wav. Here the "environmentname" is the name of the environment (EN), and the "\_image" and "\_audio" are the file descriptors (FD). The EN needs to be specified in the JSON file, but not the FD or file extensions. Make sure that each image file has a corresponding audio file, and both need to share exactly the same EN. **Maximum supported resolution for images is 16384x16384.**

Can pre-define messages available for Reseachers to send Participants, in the *messages.json* file. Examples present in the file by default. Copy the structure between curly brackets to define a new message and its responses. If you want the message to not have responses, delete the content between the square brackets (but not the square brackets themselves). **messages.json file needs to be on the Reasercher computer.**

Can pre-define the questionnaire to be presented to the Participant upon request, in the *questionnaire.json* file. Examples present in the file by default, and you can replicate the structure to define your own. **questionnaire.json file needs to be on the Participant computer.** Submitted responses are saved in a .csv file named after the submission time-stamp, and is placed in the *StreamingAssets\SavedData* folder **on the Participant computer.** Upon the Participant submitting their responses, the Researcher instance is notified of whether they have been successfully saved.  

<br/>

-------

<br/>

## Local Experiment
### Description
1. System that takes textures and audio from the Streamingassets folder at runtime and presents them as virtual environments in the order defined in the *EnvironmentOrder.json* file.
2. Facilitates presenting questionnaires on a UI rendered on a 3D virtual smartphone at the end of every timed virtual environment presentation.
3. Implements a Finite-State Machine (FSM) for automating the experiment process.


### Use
Open as Unity project, open the *LocalExperimentScene* Scene, build the project for PC Standalone with Windows as the target platform, and run. Not tested on other platforms, but may also work as intended. Likely incompatibilities include folder paths and file loading/saving, and functionalities related to the VR headset. 
Currently each environment is presented for 180 seconds, and transitional environments for 35 seconds. This can be changed through the *PlayerLC* object in the project hierarchy, via the Unity editor.

Camera movement is controlled via an HMD, and UI navigation using a gamepad controller (XBOX One controller confirmed). UI interaction also works with a mouse.

The environments to be presented are located in the *StreamingAssets/Environments* folder, and the order in which they are presented is defined in the *EnvironmentOrder.json* file in the same folder. **Maximum supported resolution for images is 16384x16384.**

The questionnaire to be presented is defined in *craving_questionnaire.json*. Examples present in the file by default, and you can replicate the structure to define your own. Submitted responses are saved in a .csv file named after the username used at login, followed by "_responses". and is placed in the *SavedData* folder.

<br/>

-------

<br/>

## System-Use Tutorial
### Description
1. System that aims to familiarize partiicpants with using an HMD to view virtual environments, and instructs them on how to use the UI to interact with sliders and buttoms.
2. First has users locate an object outside current view of the camera three times.
3. Then presents a sample UI and informs how one can interact with it using a gamepad controller.
4. Finally, performs a small test by having participants change a specific slider value to a given one, and to press a specific button. 

### Use
Open as Unity project, open the *InstructionsScene* Scene, build the project for PC Standalone with Windows as the target platform, and run. Not tested on other platforms, but may also work as intended. Likely incompatibilities include folder paths and file loading/saving, and functionalities related to the VR headset. 
 
