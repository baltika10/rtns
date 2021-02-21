namespace RTNS.Core.Model
{
    using System;

    public class Subscriber : IEquatable<Subscriber>
    {
        public Subscriber(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(nameof(id));

            Id = id;
        }

        public string Id { get; }

        public bool Equals(Subscriber other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Subscriber) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Subscriber left, Subscriber right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Subscriber left, Subscriber right)
        {
            return !Equals(left, right);
        }
    }
}
