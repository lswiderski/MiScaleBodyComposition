namespace MiScaleBodyComposition.Contracts
{
    public class User
    {
        public User(int height, int age, Sex sex)
        {
            Height = height;
            Age = age;
            Sex = sex;
        }

        public int Height { get; private set; }
        public int Age { get; private set; }
        public Sex Sex { get; private set; }
    }
}
