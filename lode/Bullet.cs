using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using lode;
using System.Reflection.Emit;
using System.Net;
public class Bullet
{
    Texture2D BulletTexture, Explosion1, Explosion2, Explosion3;
    Vector2 pos, direction;
    int state = 0;
    float timer;
    const float TIMEOFEXPLOSION = 0.1f;
    const int speed = 300;
    Vector2 nextPos;
    public bool isDead = false;

    public Bullet(Vector2 pos, Vector2 direction)
    {
        this.pos = pos;
        this.direction = direction;
        BulletTexture = Game1.BulletTexture;
        Explosion1 = Game1.Explosion1;
        Explosion2 = Game1.Explosion2;
        Explosion3 = Game1.Explosion3;
    }
    public void Update(double delta)
    {
        if (isDead) return;
        if (state == 0)
        {
            nextPos = pos + direction * speed * (float)delta;
            if (!checkCollision(nextPos))
            {
                pos = nextPos;
            }
            else
            {
                timer = TIMEOFEXPLOSION;
                state = 1;
            }
        }
        else if (timer > 0)
        {
            timer -= (float)delta;
        }
        else if (timer <= 0)
        {
            if (state == 3) { isDead = true; return; }
            state++;
            timer = TIMEOFEXPLOSION;
        }
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
    }//(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!isDead)
        {
            if (state == 0)
                spriteBatch.Draw(BulletTexture, pos, null,Color.White,0,new Vector2(BulletTexture.Width/2,BulletTexture.Height/2),0.5f,SpriteEffects.None,0f);
            else if (state == 1)
                spriteBatch.Draw(Explosion1, pos, null,Color.White,0,new Vector2(Explosion1.Width/2,Explosion1.Height/2),0.5f,SpriteEffects.None,0f);
            else if (state == 2)
                spriteBatch.Draw(Explosion2, pos, null,Color.White,0,new Vector2(Explosion2.Width/2,Explosion2.Height/2),0.5f,SpriteEffects.None,0f);
            else if (state == 3)
                spriteBatch.Draw(Explosion3, pos, null,Color.White,0,new Vector2(Explosion3.Width/2,Explosion3.Height/2),0.5f,SpriteEffects.None,0f);
        }
    }
}