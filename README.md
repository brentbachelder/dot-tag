# DotTag
### Realtime Multiplayer Tag Game using Firebase
Language: *C#*
Framework: *Unity*
Database: *Firebase*

## Goal of the Project
I wanted to see if I could create a multiplayer game using only the free tier of Firebase. The game is a simple "tag game" using dots.


## How to Use
You and a couple friends have to install the APK file on your Android phones.


## Problems to Solve In this Project
The biggest problem (or so I thought) was going to be the speed at which the game called player coordinates from Firebase. Turns out, Firebase is surprisingly fast... so that issue was resolved quickly. The first big puzzle ended up being the network usuage of a game. The free tier allows for 350MB per day, and after running six players for one minute on the first run-through, I had already used roughly 36MB. This was wayyy too much for a single game.

Initially, the variables that were called on each frame was the float position x, float position y, boolean of whether the player was "it", the players dot color, and a boolean for whether the player was dead (had been tagged 3 times). First I removed the color from being called every frame, then rounded the player position floats to the hundreth and converted to integers. For example, if a player was at the coordinates 2.43513411, 0.234994... I rounded to the nearest hundreth and multiplied by a hundred to create an integer. So the above example would result in the coordinates 244, 23. This led to a negligible difference when being converted back for position updates. Then I added a 1 or 0 to the end for whether they were "it", and a 1 or 0 for whether they were dead. So an 8 bit integer was being uploaded and downloaded each frame that looked like this:
  x-Pos   y-Pos   It (false)  Dead(false)
   244     023       0            0           = 24402300
   
The other thing was to create an internal 3 frame buffer to make network calls 20x per second instead of 60x. To make this work without lag and for as close to correct positioning as possible, I had to use predictive movement. It's hard to explain without seeing it, but ultimately FRAME 1 would take the direction of the controller and multiple the movement speed by two in that direction and send those coordinates. The other players would receive those coordinates on FRAME 2, set that as the position for FRAME 3 (so now the original player is actually in the position that was sent), and then send the next group of coordinates.

Finally, the last (and actually hardest problem to solve) was the fact that players use this on mobile. I had to account for a player pausing the app without closing it, closing the app but still letting the network know, resuming after pausing without losing positions, and the general game management of one player controlling the game who could leave and everyone would be stranded. What a pain. When I create this game (or a game similar to it) for release, I know 99% of the bug reports will be this part of the application.


## The Takeaway
Programmers have to have a certain type of analytical brain to be programmers. This was a great experiment to really push those boundaries for myself. I hope that someday I'll be able to use this in a production environment where I can save a company a bunch of money by recognizing that we can limit our databse calls or change a file structure to transmit smaller files.
