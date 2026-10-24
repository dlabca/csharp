using System;
using Microsoft.Xna.Framework;

namespace open_world
{
    // Pes chodí, sbírá kachny ležící na zemi (stejný seznam jako by sbíral
    // hráč - viz DuckManager._groundedDuckIndices) a nese je za hráčem.
    // Peníze se připisují až v Delivering, ne při sebrání - tzn. "prodej"
    // je vázaný na doručení, ne na samotný úlovek.
    //
    // Placeholder vykreslení: dokud nemáš model, kresli si Position jako
    // jednoduchý box/kapsli přes BasicEffect - tahle třída se drží jen
    // logiky a pozice, vykreslování je oddělené (stejně jako u kachen).
    public class Dog
    {
        private enum State { Following, Seeking, Delivering }
        private State _state = State.Following;

        public Vector3 Position;
        public Vector3 FacingDirection { get; private set; } = Vector3.UnitZ;
        public int CarriedDucks { get; private set; }

        private Vector3 _seekTarget;

        // Jak daleko dohlédne po ležící kachně - ať neběhá přes půl mapy
        // za úlovkem, kterého si všimnul před pěti minutami.
        private const float SearchRadius = 3000f;
        private const float PickupRadius = 5f;
        private const float DeliverRadius = 5f;

        // Pozice "u nohy" hráče - kus za ním a kousek stranou (napravo),
        // počítáno vůči tomu, kam hráč kouká (playerFacing), ne vůči
        // pevným světovým osám - jinak by pes zůstal viset na jednom
        // místě vždycky, když se hráč otočí.
        private const float HeelOffsetBack = -1.8f;
        private const float HeelOffsetSide = 1.4f;

        public void Update(float deltaTime, float totalTime, Vector3 playerPosition, Vector3 playerFacing, DuckManager duckManager)
        {
            switch (_state)
            {
                case State.Following:
                    UpdateFollowing(playerPosition, playerFacing, deltaTime, duckManager);
                    break;

                case State.Seeking:
                    UpdateSeeking(deltaTime, totalTime, duckManager);
                    break;

                case State.Delivering:
                    UpdateDelivering(playerPosition, deltaTime);
                    break;
            }

            Position.Y = TerrainGenerator.GetHeight(Position.X, Position.Z);
        }

        private void UpdateFollowing(Vector3 playerPosition, Vector3 playerFacing, float deltaTime, DuckManager duckManager)
        {
            bool hasCapacity = CarriedDucks < DogEconomy.CurrentCapacity;

            if (hasCapacity && duckManager.TryGetNearestGroundedDuck(Position, SearchRadius, out _seekTarget))
            {
                _state = State.Seeking;
                return;
            }

            if (CarriedDucks > 0)
            {
                // Nic dalšího v dosahu (nebo plno) - nes co máš k hráči.
                _state = State.Delivering;
                return;
            }

            MoveToward(FollowTargetPosition(playerPosition, playerFacing), deltaTime);
        }

        private void UpdateSeeking(float deltaTime, float totalTime, DuckManager duckManager)
        {
            MoveToward(_seekTarget, deltaTime);

            if (Vector3.DistanceSquared(Position, _seekTarget) <= PickupRadius * PickupRadius)
            {
                if (duckManager.TryPickupGroundedDuck(Position, PickupRadius, totalTime))
                    CarriedDucks++;

                // Zpátky do Following - tam se rozhodne, jestli jít pro další,
                // nebo (při plné kapacitě / prázdném okolí) doručit.
                _state = State.Following;
            }
        }

        private void UpdateDelivering(Vector3 playerPosition, float deltaTime)
        {
            MoveToward(playerPosition, deltaTime);

            if (Vector3.DistanceSquared(Position, playerPosition) <= DeliverRadius * DeliverRadius)
            {
                GameEconomy.Money += GameEconomy.DuckValue * CarriedDucks;
                CarriedDucks = 0;
                _state = State.Following;
            }
        }

        private void MoveToward(Vector3 target, float deltaTime)
        {
            Vector3 flatDelta = new Vector3(target.X - Position.X, 0, target.Z - Position.Z);
            float dist = flatDelta.Length();
            if (dist < 0.01f) return;

            FacingDirection = flatDelta / dist;

            float step = DogEconomy.CurrentSpeed * deltaTime;
            if (step >= dist)
                Position = new Vector3(target.X, Position.Y, target.Z);
            else
                Position += flatDelta / dist * step;
        }

        // "U nohy" = kousek za hráčem a kousek napravo, vztaženo k tomu,
        // kam hráč kouká (stejná right = Cross(forward, up) konvence jako
        // ve WeaponRenderer, ať to sedí s natočením kamery).
        private Vector3 FollowTargetPosition(Vector3 playerPosition, Vector3 playerFacing)
        {
            Vector3 forward = new Vector3(playerFacing.X, 0, playerFacing.Z);
            if (forward.LengthSquared() < 0.0001f)
                forward = Vector3.UnitZ;
            forward.Normalize();

            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.Up));

            return playerPosition - forward * HeelOffsetBack + right * HeelOffsetSide;
        }
    }
}