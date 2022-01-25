namespace CodeVsZombiesLibrary
{
    public struct ZombieInputs
    {
        public int Id { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int XNext { get; private set; }
        public int YNext { get; private set; }

        public ZombieInputs(int id, int x, int y, int xNext, int yNext)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
            this.XNext = xNext;
            this.YNext = yNext;
        }

        public override string ToString()
        {
            return $"{Id} {X} {Y} {XNext} {YNext}";
        }
    }
}