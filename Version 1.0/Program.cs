using System;
using System.Numerics;
using Raylib_cs;
using static Global;

class Global
{
    public const int WINW = 1024;
    public const int WINH = 512;
}

class Fish(short[] SHAPE, Color Fishcolor, Color Highlight)
{
    public Vector2[] Positions = new Vector2[SHAPE.Length]; // segment positions
    public Vector2 Target; // where its going to follow 
    public short[] FishShape = SHAPE; // the radii of the fishies
    Color FishColor = Fishcolor; // color of fish
    Color FishHighLight = Highlight; // highlight of fish

    // NORMALS
    private Vector2 Normal;
    public Vector2[] LeftNormals = new Vector2[SHAPE.Length + 1];
    public Vector2[] RightNormals = new Vector2[SHAPE.Length + 1];

    // Angle Borders
    private static float maxAngle = MathF.PI / 2; // 90 degrees π/2

    // DRAWING EYES
    public static Vector2 eyeLeft, eyeRight, eyeNormal;
    public static Vector2 direction;

    // USING SPLINES
    Vector2 LastNormal;


    // FUNCTIONS

    private readonly Random r = new(); // radnom generator
    public void DefineFish()
    {
        for (int i = 0; i < Positions.Length; i++)
        {
            // Create Random Target
            Target = new(Raylib.GetRandomValue(50, WINW),Raylib.GetRandomValue(50, WINH));

            // For the first segment --> follow target
            Positions[i] = Target;
            
            if (i == 0) continue; // if its not the first segment run the following code
            
            // set the segment to a random position
            Positions[i] = new(-100,-100);
        }
        
    }

    private static float curAngle = 0;
    public void UpdateFish(float eyesDistance, float eyeRadius) // put into a for loop
    {   
        // First segment is the target;
        Positions[0] = Target;

        // Update the target 
        if (Raylib.CheckCollisionPointCircle(Positions[0], Positions[1], FishShape[1]))
        {
            Raylib.PlaySound(Program.waterDrop); // make sound effect 
            Target = new(Raylib.GetRandomValue(-150, WINW+150),Raylib.GetRandomValue(-150, WINH+150));
        }

        // Update the second segment and draw its eyes
        direction = Vector2.Normalize(Target - Positions[1]); // find out the direction
        eyeNormal = FindNormal(direction); // find out the normal

        // Figure out the left and right eye coords
        // EYECORD = HeadPosition + direction * eyeDistance +- Normal * Radius
        eyeLeft = Positions[1] + direction * eyesDistance + eyeNormal * FishShape[1];
        eyeRight = Positions[1] + direction * eyesDistance - eyeNormal * FishShape[1];

        // Last NORMAL ==> last segment

        // direction is second-last and last using INDEX OPERATORS
        direction = Vector2.Normalize(Positions[^2] - Positions[^1]);
        LastNormal = new(-direction.X * FishShape[^1], -direction.Y * FishShape[^1]) ; // FLIP IT OTHER DIRECTION

        // SETTING THE LAST NORMALS
        RightNormals[^1] = LastNormal + Positions[^2];
        LeftNormals[^1] = LastNormal + Positions[^2];

        // FOR SMOOTHING HEAD OUTLINE
        RightNormals[0] = eyeRight;
        LeftNormals[0] = eyeLeft;

        

        for (int index = 1; index < Positions.Length; index++) // skip the first segment --> has eyes
        {
            // Updating Segments

            // set new direction
            direction = Vector2.Normalize(Positions[index-1] - Positions[index]);

            // updating normals

            // Find original normal
            Normal = FindNormal(direction);
            
            // Find left and right normals
            LeftNormals[index] = Positions[index] + direction + Normal * FishShape[index];
            RightNormals[index] = Positions[index] + direction - Normal * FishShape[index];

            // Get the current Angle
            curAngle = FindAngle(Positions[index], Positions[index - 1]);

            // Set a distance constraint --> move closer if too far away
            if (Vector2.Distance(Positions[index-1], Positions[index]) > (FishShape[index]) && curAngle < maxAngle)
            {
                Positions[index] += direction * 3; // move by direction times 3, speed of 3
            } 

            // OUTLINE ONLY THE FIRST ONE
            if (index == 1) {
            Raylib.DrawCircleV(Positions[index],FishShape[index] * 1.3f, FishHighLight);}

            // SEGMENTS
            Raylib.DrawCircleV(Positions[index], FishShape[index], FishColor); // draw segment
        

            // OUTLINE ALL SEGMENTS
            Raylib.DrawSplineBezierQuadratic(LeftNormals, LeftNormals.Length, 7f, FishHighLight);
            Raylib.DrawSplineBezierQuadratic(RightNormals, RightNormals.Length, 7f, FishHighLight);

            
        }

        // Drawing eyes 

        // Sclera --> White part
        Raylib.DrawCircleV(eyeLeft, eyeRadius, Color.White);
        Raylib.DrawCircleV(eyeRight, eyeRadius, Color.White);

        // Iris --> Black part
        Raylib.DrawCircleV(eyeLeft, eyeRadius/2, Color.Black);
        Raylib.DrawCircleV(eyeRight, eyeRadius/2, Color.Black);
        
    }

    public static Vector2 FindNormal(Vector2 direction)
    {
        return new(-direction.Y, direction.X);
    }

    private static float xDist, yDist;
    public static float FindAngle(Vector2 v1, Vector2 v2)
    {
        xDist = MathF.Abs(v2.X - v1.X); // x distance --> adjacent
        yDist = MathF.Abs(v2.Y - v1.Y); // y distance --> opposite

        // Find angle INVERSE TANGENT (adjacent / opposite)
        return MathF.Atan(xDist / yDist);
    }

}



class Program
{
    private static readonly Random rc = new(118);

    // SOUNDS
    public static Sound waterDrop = Raylib.LoadSound("waterSFX.mp3");
    public static Color RandomColor()
    {
        return new Color((byte)rc.NextInt64(50,100), (byte)rc.NextInt64(50,100), (byte)rc.NextInt64(50,100));
    }
    public static void Main()
    {
        // Initialize
        Image logo = Raylib.LoadImage("logo.png");

        // SETTING UP THE WINDOW
        Raylib.InitWindow(WINW, WINH, "Koi Pond");
        Raylib.SetWindowIcon(logo);
        Raylib.ToggleFullscreen();

        // FPS AND DELTATIME
        Raylib.SetTargetFPS(100);
        float dt = 0;

        // SOUNDS
        Raylib.InitAudioDevice();
        Music backgroundNoise = Raylib.LoadMusicStream("bgMusic.mp3");
        

        // FISHIES
        int totalFish = 3;

        // DIFFERENT SHAPES
        short[] KOISHAPE = [10,20,30,30,25,25,20,20,15,15,10,5, /*TAIL-->*/ 0,0,2,2,2,2,2,2,2,2,2,2,2,8,8,8,8,8,10,10,10,10,10,15];
        
        // LIST OF FISHY
        Fish[] fishy = new Fish[totalFish];

        // DEFINE ALL FISHIES
        for (int f = 0; f < totalFish; f++)
        {
            Fish fih = new(KOISHAPE, RandomColor(), Color.White); // CREATE FISH
            fih.DefineFish(); // DEFINE THE FISH   
            fishy[f] = fih; // ADD TO LIST
        }

        Raylib.PlayMusicStream(backgroundNoise); // play it
        while (!Raylib.WindowShouldClose())
        {
            dt = Raylib.GetFrameTime(); // deltatime
            
            // Background noise
            Raylib.UpdateMusicStream(backgroundNoise); // keep updating it

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.SkyBlue);

            for (int i = 0; i < totalFish; i++)
            {
                fishy[i].UpdateFish(2.5f,10); // UPDATE THE FISH   
            }

            // Info
            Raylib.DrawText("Press 'ESC' to exit", 25, 25, 25, Color.Black);



            Raylib.EndDrawing();


        }
        // UNLOADING WINDOW, SOUNDS, LOGO, AND AUDIO DEVICE
        Raylib.UnloadImage(logo);
        Raylib.UnloadMusicStream(backgroundNoise);
        Raylib.UnloadSound(waterDrop);
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }
}