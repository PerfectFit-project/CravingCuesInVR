# Craving Cues in VR

## Goals
1. Develop a tool to create personalized virtual reality environments to elicit smoking cravings.
2. Develop UI to allow researchers to communicate with participants during the experiment.

## (Rough) Plan
1. Develop UI and implement networking to facilitate communication between researcher and participant interfaces.
2. Integrate virtual environment functionality.

### Progress Status
1. Researcher and participant UI (mostly) complete in terms of functionality. 
2. Implemented a panoramic photo viewer using a camera attached to a sphere and rendering the desired texture inside it. Camera rotation using mouse movement, have not implemented VR controls yet.
3. Implemented networking: 
	1. Host (researcher) and client (participant) can communicate via the former selecting a message template (loaded from a JSON file) with acceptable responses, and the latter selecting the response desired.
	2. The client (participant) can move the camera and view the environments, while the host (researcher) can only view what the participant is viewing. (Needs some work, but basic functionality is there.)

### Use
In the present state to see the most up to date functionality, open as Unity project, open the "PanoramicEnvsNetworking" Scene, build the project for PC Standalone with Windows as the target platform, and run. Need two instances running to test the functionality, i.e. editor and an instance from the build executable, or two instances of the built executable, one being the researcher (host) and the other being the participant (client). Host passowrd is "a". Sets up a server and connects on the localhost network address.
