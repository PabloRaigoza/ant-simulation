# ant-simulation

## ball_physics:

1. Create the Ball:

    In the Hierarchy window, right-click and select 3D Object > Sphere.
    Rename the sphere to "Ball" in the Inspector.

2. Add Rigidbody to the Ball:

    With the ball selected, in the Inspector, click Add Component and search for Rigidbody.
    This makes the ball subject to Unity’s physics system (gravity, collisions, etc.).

3. Use ball_physics for Controlling the Ball:

    In the Project window, right-click and select Create > C# Script.
    copy ball_physics.cs into for the ball

4. Control:
    Use the arrow keys (or W, A, S, D) for rolling movement
    Use space bar to jump
