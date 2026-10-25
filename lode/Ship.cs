using System;
using lode;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
public class Ship
{
    public Vector2 pos;
    int damage;
    Keys MoveUp, MoveDown, MoveRight, MoveLeft, ShootRight, ShootLeft;
    KeyboardState keyboard;
    Texture2D ShipDmg0, ShipDmg1, ShipDmg2, ShipDead;
    const int SPEED = 5;
    const int MAXSPEED = 100;
    const float SCALE = 0.5f;
    Vector2 velocity = Vector2.Zero;
    float rotation;
    bool wasShootLeft;
    bool wasShootRight;

    public Ship(Vector2 Pos, int rotation, int Damage, Keys MvUp, Keys MvDown, Keys MvRight, Keys MvLeft, Keys ShootRight, Keys ShootLeft, Texture2D Sdmg0, Texture2D Sdmg1, Texture2D Sdmg2, Texture2D ShipD)
    {
        pos = Pos;
        damage = Damage;
        MoveUp = MvUp;
        MoveDown = MvDown;
        MoveRight = MvRight;
        MoveLeft = MvLeft;
        this.ShootRight = ShootRight;
        this.ShootLeft = ShootLeft;
        ShipDmg0 = Sdmg0;
        ShipDmg1 = Sdmg1;
        ShipDmg2 = Sdmg2;
        ShipDead = ShipD;
        this.rotation = rotation;
    }
    public void Update(double delta)
    {
        keyboard = Keyboard.GetState();
        Vector2 direction = Vector2.Zero;

        if (keyboard.IsKeyDown(this.MoveRight)) direction += new Vector2(1, 0);
        if (keyboard.IsKeyDown(this.MoveLeft)) direction += new Vector2(-1, 0);
        if (direction != Vector2.Zero)
        {
            velocity.X += direction.X * SPEED;
            velocity.X = Vector2.Clamp(velocity, new Vector2(-MAXSPEED, 0), new Vector2(MAXSPEED, 0)).X;
            direction = Vector2.Zero;
        }
        else velocity.X *= 0.95f;
        if (keyboard.IsKeyDown(this.MoveUp)) direction += new Vector2(0, -1);
        if (keyboard.IsKeyDown(this.MoveDown)) direction += new Vector2(0, 1);
        if (keyboard.IsKeyDown(this.ShootLeft))
        {
            if (wasShootLeft == false)
            {
                Shoot(1);
                wasShootLeft = true;
            }
        }
        else wasShootLeft = false;
        if (keyboard.IsKeyDown(this.ShootRight))
        {
            if (wasShootRight == false)
            {
                Shoot(-1);
                wasShootRight = true;
            }
        }
        else wasShootRight = false;

        if (direction != Vector2.Zero)
        {
            velocity.Y += direction.Y * SPEED;
            velocity.Y = Vector2.Clamp(velocity, new Vector2(0, -MAXSPEED), new Vector2(0, MAXSPEED)).Y;
        }
        else velocity.Y *= 0.95f;

        if (velocity.Length() < SPEED)
            velocity = Vector2.Zero;

        if (checkCollision(pos + new Vector2(velocity.X * (float)delta, 0))) velocity.X = 0;
        if (checkCollision(pos + new Vector2(0, velocity.Y * (float)delta))) velocity.Y = 0;

        pos += velocity * (float)delta;
        if (velocity != Vector2.Zero)
            rotation = MathF.Atan2(velocity.Y, velocity.X);

    }
    private bool checkCollision(Vector2 targetPosition)
    {
        int tileX = (int)targetPosition.X / Game1.tileSize;
        int tileY = (int)targetPosition.Y / Game1.tileSize;

        foreach (var item in Game1.islands)
        {
            if (tileX > item[0] && tileX < item[1] && tileY > item[2] && tileY < item[3])
            {
                return true;
            }
        }
        return false;
    }
    void Shoot(int direction)
    {
        Game1.bullets.Add(new Bullet(pos, new Vector2((float)Math.Cos(rotation - 5 * MathHelper.PiOver2), (float)Math.Sin(rotation - 5 * MathHelper.PiOver2)) * direction));
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        if (damage == 0)
            spriteBatch.Draw(ShipDmg0, new Rectangle((int)pos.X, (int)pos.Y, (int)(66 * SCALE), (int)(113 * SCALE)), null, Color.White, rotation - MathHelper.PiOver2, new Vector2((int)(66 * SCALE), (int)(113 * SCALE)), SpriteEffects.None, 0f);
        else if (damage == 1)
            spriteBatch.Draw(ShipDmg1, new Rectangle((int)pos.X, (int)pos.Y, (int)(66 * SCALE), (int)(113 * SCALE)), null, Color.White, rotation - MathHelper.PiOver2, new Vector2((int)(66 * SCALE), (int)(113 * SCALE)), SpriteEffects.None, 0f);
        else if (damage == 2)
            spriteBatch.Draw(ShipDmg2, new Rectangle((int)pos.X, (int)pos.Y, (int)(66 * SCALE), (int)(113 * SCALE)), null, Color.White, rotation - MathHelper.PiOver2, new Vector2((int)(66 * SCALE), (int)(113 * SCALE)), SpriteEffects.None, 0f);
        else if (damage == 3)
            spriteBatch.Draw(ShipDead, new Rectangle((int)pos.X, (int)pos.Y, (int)(66 * SCALE), (int)(113 * SCALE)), null, Color.White, rotation - MathHelper.PiOver2, new Vector2((int)(66 * SCALE), (int)(113 * SCALE)), SpriteEffects.None, 0f);
    }
}