using System;
using System.Data;
//using System.Drawing;
//using System.Numerics;
using System.Xml.Serialization;
using lode;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Net.Mime;
class Ship
{
    Vector2 pos;
    int damage;
    Keys MoveUp, MoveDown, MoveRight, MoveLeft;
    KeyboardState keyboard;
    Texture2D ShipDmg0, ShipDmg1, ShipDmg2;
    const int SPEED = 5;
    const int MAXSPEED = 100;
    const float SCALE = 0.5f;
    Vector2 velocity = Vector2.Zero;
    float rotation;
    float aktualSpeed = 0;

    public Ship(Vector2 Pos, int rotation, int Damage, Keys MvUp, Keys MvDown, Keys MvRight, Keys MvLeft, Texture2D Sdmg0, Texture2D Sdmg1, Texture2D Sdmg2)
    {
        pos = Pos;
        damage = Damage;
        MoveUp = MvUp;
        MoveDown = MvDown;
        MoveRight = MvRight;
        MoveLeft = MvLeft;
        ShipDmg0 = Sdmg0;
        ShipDmg1 = Sdmg1;
        ShipDmg2 = Sdmg2;
        this.rotation = rotation;
    }
    public void Update(double delta)
    {
        keyboard = Keyboard.GetState();
        Vector2 direction = Vector2.Zero;

        if (keyboard.IsKeyDown(this.MoveRight))
        {
            direction += new Vector2(1, 0);
        }
        if (keyboard.IsKeyDown(this.MoveLeft))
        {
            direction += new Vector2(-1, 0);
        }
        if (direction != Vector2.Zero)
        {
            velocity.X += direction.X * SPEED;
            velocity.X = Vector2.Clamp(velocity, new Vector2(-MAXSPEED, 0), new Vector2(MAXSPEED, 0)).X;
            direction = Vector2.Zero;
        }
        else velocity.X *= 0.95f;
        //------------------------------------------

        if (keyboard.IsKeyDown(this.MoveUp))
        {
            direction += new Vector2(0, -1);

        }
        if (keyboard.IsKeyDown(this.MoveDown))
        {
            direction += new Vector2(0, 1);

        }
        //Console.WriteLine(direction);
        if (direction != Vector2.Zero)
        {
            velocity.Y += direction.Y * SPEED;
            velocity.Y = Vector2.Clamp(velocity, new Vector2(0, -MAXSPEED), new Vector2(0, MAXSPEED)).Y;
        }
        else velocity.Y *= 0.95f;
        aktualSpeed = velocity.Length();
        if (aktualSpeed < SPEED)
            velocity = Vector2.Zero;
        pos += velocity * (float)delta;
        if (velocity != Vector2.Zero)
            rotation = MathF.Atan2(velocity.Y, velocity.X);

    }
    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(ShipDmg0, new Rectangle((int)pos.X, (int)pos.Y, (int)(66 * SCALE), (int)(113 * SCALE)), null, Color.White, rotation - MathHelper.PiOver2, new Vector2((int)(66 * SCALE), (int)(113 * SCALE)), SpriteEffects.None, 0f);
    }
}