namespace RTNS.Core.Model
{
    using System;

    public class Topic : IEquatable<Topic>
    {
        public Topic(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            Name = name;
        }

        public string Name { get; }

        public bool Equals(Topic other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Topic) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(Topic left, Topic right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Topic left, Topic right)
        {
            return !Equals(left, right);
        }
    }
}
