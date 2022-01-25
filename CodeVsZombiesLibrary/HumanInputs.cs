namespace CodeVsZombiesLibrary
{
    public struct HumanInputs
    {
        public int Id { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public HumanInputs(int id, int x, int y)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return $"{Id} {X} {Y}";
        }
    }
}