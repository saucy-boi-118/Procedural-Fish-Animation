using System;
using System.Numerics;
using Raylib_cs;
using static Global;
using static Fish;

class Global
{
    public const int WINW = 1024, WINH = 512;

    // fix these /  add modes.
    public static short[] FishSizes = [10,20,30,30,25,25,20,20,15,15,10,5];
    static readonly float angleDivider = 57.2957795f;
    public static float angle25 = 25 / angleDivider;
    public static float angle90 = 90 / angleDivider;
}

// ADD ANGLE CONSTRAINT
class Fish(int Length, Color Fishcolor, Color Highlight, Vector2 Target)
{
    public Vector2[] Positions = new Vector2[Length]; // segment positions
    public Vector2 Target = Target; // where its going to follow 
    public short[] FishShape = new short[Length]; // the radii of the fishies
    Color FishColor = Fishcolor; // color of fish
    Color FishHighLight = Highlight; // highlight of fish

    // NORMALS
    private static readonly int Columns = 2;
    public Vector2[] normals = new Vector2[Length * Columns];

    // DRAWING EYES
    public static Vector2 eyeLeft, eyeRight, eyeNormal;
    public static Vector2 direction;

    // FUNCTIONS

    private readonly Random r = new(); // radnom generator
    public void DefineFish(Fish fish)
    {
        for (int i = 0; i < Positions.Length; i++)
        {

            // For the first segment --> follow target
            Positions[i] = Target;
            
            if (i == 0) continue; // if its not the first segment run the following code
            
            // set the segment to a random position
            Positions[i] = new((float) r.NextInt64(50, WINW - 50), (float) r.NextInt64(50, WINH - 50));
        }
        
    }

    public int UpdateFish(Fish fish, int index, float eyesDistance, float eyeRadius) // put into a for loop
    {
        // First segment is the target --> starting point for the other segments to follow
        Positions[index] = Target;

        if (index == 0) return 1; // stop running this function if index is 0
        
        // Update the second segment and draw its eyes
        direction = Vector2.Normalize(Target - Positions[index]); // find out the direction
        eyeNormal = FindNormal(direction); // find out the normal

        // Figure out the left and right eye coords
        // EYECORD = HeadPosition + direction * eyeDistance +- Normal * Radius
        eyeLeft = Positions[index] + direction * eyesDistance + eyeNormal * FishShape[index];
        eyeRight = Positions[index] + direction * eyesDistance - eyeNormal * FishShape[index];

        // Drawing eyes 

        // Sclera
        Raylib.DrawCircleV(eyeLeft, eyeRadius, Color.Black);
        Raylib.DrawCircleV(eyeRight, eyeRadius, Color.Black);

        // Iris
        Raylib.DrawCircleV(eyeLeft, eyeRadius/2, Color.White);
        Raylib.DrawCircleV(eyeRight, eyeRadius/2, Color.White);


        // Updating Segments

        // set new direction
        direction = Vector2.Normalize(Positions[index] - Positions[index-1]);

        // Set a distance constraint --> move closer if too far away
        if (Vector2.Distance(Positions[index-1], Positions[index]) > (FishShape[index]))
        {
            Positions[index] += direction * 3; // move by direction times 3, speed of 3
        } 

        Raylib.DrawCircleV(Positions[index], FishSizes[index], FishColor); // draw segment


        return 0;
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
class Progam
{
    public struct ProceduralCirclePart
    {
        public Vector2 Position;
        public short radius;
    }

    public static float FindPointsAngle(ProceduralCirclePart p1, ProceduralCirclePart p2)
    {
        float adjacent = MathF.Abs(p1.Position.X - p2.Position.X);
        float hypotenuse = Vector2.Distance(p1.Position, p2.Position);
        return MathF.Acos(adjacent / hypotenuse);
    }
    public static void Main()
    {
        // INIT
        Raylib.InitWindow(WINW, WINH, "Fih");
        Raylib.SetTargetFPS(100);
        float dt;


        // Variables
        ProceduralCirclePart[] parts = new ProceduralCirclePart[FishSizes.Length];
        Vector2 Center = new(WINW / 2, WINH / 2);

        // defining circles
        int i;
        for (i = 0; i < parts.Length; i++)
        {
            // define
            parts[i].Position = Raylib.GetMousePosition();
            parts[i].radius = FishSizes[i];
            if (i == 0) continue;
            parts[i].Position = new(Raylib.GetRandomValue(50, WINW - 50), Raylib.GetRandomValue(50, WINH - 50));
            parts[i].radius = FishSizes[i];
        }

        Vector2 dir;

        // Line drawing 
        Vector2 leftEye, rightEye, normal; 

        Vector2 leftNorm,rightNorm;
       // Vector2 pastNormL,pastNormR;

        float maxAngle = angle90;
        float eyeRadius = 5;

        // Looop
        while(!Raylib.WindowShouldClose())
        {
            dt = Raylib.GetFrameTime();

            // update
            parts[0].Position = Raylib.GetMousePosition(); // follow cursor

            // eyes
            dir = Vector2.Normalize(parts[0].Position - parts[1].Position);
            normal = new(-dir.Y, dir.X);
            leftEye = parts[1].Position + dir * 5 + normal * (parts[1].radius);
            rightEye = parts[1].Position + dir * 5 - normal * (parts[1].radius);


            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DarkBlue);

            for (i = parts.Length - 1; i > 0; i--)
            {        
                
                Raylib.DrawCircleV(parts[i].Position, parts[i].radius, Color.Purple); // draw
                if (i == 0) continue; // with the loop

                // if its farther then the radii
                dir = Vector2.Normalize(parts[i-1].Position - parts[i].Position);

                if (Vector2.Distance(parts[i-1].Position, parts[i].Position) > (parts[i].radius) &&
                    FindPointsAngle(parts[i], parts[i-1]) < maxAngle) 
                {
                    parts[i].Position += dir * 3;
                } 

                Raylib.DrawCircleV(parts[i].Position, parts[i].radius, Color.Purple); // draw

                normal = new(-dir.Y, dir.X);
                leftNorm = parts[i].Position + dir * 35 + normal * (parts[i].radius);
                rightNorm = parts[i].Position + dir * 35 - normal * (parts[i].radius);

                if (i == (parts.Length / 2) - 2)
                {
                    // FRONT FINS
                    Raylib.DrawEllipse((int)leftNorm.X, (int)leftNorm.Y, parts[i].radius/2*1.5f, parts[i].radius/2, Color.Pink);
                    Raylib.DrawEllipse((int)rightNorm.X, (int)rightNorm.Y, parts[i].radius/2*1.5f, parts[i].radius/2, Color.Pink);    
                }

                if (i == parts.Length - 2)
                {
                    // BACK FINS
                    Raylib.DrawEllipse((int)leftNorm.X, (int)leftNorm.Y, parts[i].radius*1.5f, parts[i].radius, Color.Pink);
                    Raylib.DrawEllipse((int)rightNorm.X, (int)rightNorm.Y, parts[i].radius*1.5f, parts[i].radius, Color.Pink);   
                }

                /*
                pastNormL = parts[i-1].Position + dir * 35 + normal * (parts[i-1].radius);
                pastNormR = parts[i-1].Position + dir * 35 - normal * (parts[i-1].radius);

                 // outline
                Raylib.DrawLineBezier(leftNorm, pastNormL, 15f, Color.White);
                Raylib.DrawLineBezier(rightNorm, pastNormR, 15f, Color.White);
                */           
            }

            // sclera
            Raylib.DrawCircleV(leftEye, eyeRadius, Color.Black);
            Raylib.DrawCircleV(rightEye, eyeRadius, Color.Black);

            // iris
            Raylib.DrawCircleV(leftEye, eyeRadius/2, Color.White);
            Raylib.DrawCircleV(rightEye, eyeRadius/2, Color.White);

            // ADD FINS AND ACTUALY COLOR

            Raylib.EndDrawing();
            
        }

        // Closing / Unloading

        Raylib.CloseWindow();
    }
}