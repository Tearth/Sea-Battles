# SeaBattles
This is a prototype of the game about sea battles. It's not finished but contains a basic model of the ships based on voxels. The ship is built from the voxels in the editor, and then they are merged at the runtime (vertices and colliders). These merged elements are dynamically split when they are hit by the cannonball. In the place of the hole there are instantiated independent voxels with enabled rigidbody component.

# Examples
![Example1](https://i.imgur.com/hI0SJgI.png)

Ship in the editor.

![Example2](https://i.imgur.com/hklGUgP.png)

Ship in the runtime (with merged vertices and colliders).

![Example3](https://i.imgur.com/7dbQEfv.png)

Destroyed side of the ship.

![Example4](https://i.imgur.com/oRbILdh.png)

Even more destroyed ship.

![Example5](https://raw.githubusercontent.com/Tearth/SeaBattles/master/shipgif.gif)

Example of destroying ship.