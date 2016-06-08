# Kinect-Lego

Made during a school project and linking the use of a Kinect and a Lego Ev3 Mindstorm.
Basically, you draw something in front of the Kinect with your hand, and then a connected Ev3 Mindstorm Lego robot draws the picture on a piece of paper on the floor.

## Requirements

The Kinect SDK. This project was made with a first generation Kinect. Not idea if it is still compatible with the new ones.

On the interface, you have to verify a Kinect is connected and then connect a robot (bluetooth works best).
On the right side is the Kinect stream, whereas on the left side you will see the lines that will be drew by the robot.

The Lego Ev3 Mindstorm robot was a custom build, having some kind of arm that could hold a pen, and lift up and down.

Some ajustments are missing. The turning is calibrated rightly. While time passes, the robot will slowly divert from his theoretical path.
This might be linked to the brakes.

The robot assumes to start in the middle of the paper. You can tell the software the size of the piece of paper (or cardboard btw)

## The interface

* Start: launches the Kinect stream
* Stop: stops the Kinect stream
* Connect Lego: opens a dialog to connect a Lego robot
* Clear: deletes the current drawing
* End: ends the application, obviously
* Draw buttons: if connected, will convert the drawing into a drawable one for the robot and it will be launched

![Kinect Interface](/KinectInterface.png)

## The projects

### WPFKinect

The containing the GUI and handling the Kinect stream (so it can be displayed on the interface).

### Lego.Ev3.Desktop

The SDK of the Lego robot is included in the source code, yes. It was easier at that time. No idea if it has evolved.

### DrawKinect

Handles the drawing in front of the Kinect. `Draw` is a _singleton_, holding the list of segments.

But because a Kinect generates a lot of frames of seconds. That's why we have to merge together some points and take all of them.
Otherwise, the robot would you turn around the whole time.
We have two techniques for merging them together. You can choose one during runtime.

### RobotCore

This handle the robot movements. Be careful about mapping correclty the robot motors

## TO-DO

* Translating everything into english
* Ajusting the robot movements

But I still don't have the money to buy a Kinect and such a Lego robot :D
