using System.Collections.ObjectModel;

namespace Mistware.Utils
{

    /// <summary>
    /// Holds a collection of Key/Value objects
    /// KeyList and KeyPair classes are quite simlar to other classes, but their use can result in simpler code. 
    /// </summary>
    public class KeyList : Collection<KeyPair>
    {
        /// <summary>
        /// Add a Pair to the Pairs collection
        /// </summary>
        public void Add(string key, string value)
        {
            KeyPair pair = new KeyPair(key, value);
            base.Add(pair);
        }
    }
    
    /// <summary>
    /// Holds a collection of Pair objects
    /// KeyList and KeyPair classes are quite simlar to other classes, but their use can result in simpler code. 
    /// </summary>
    public class KeyList<T,S> : Collection<KeyPair<T,S>>
    {
        /// <summary>
        /// Add a Pair to the Pairs collection
        /// </summary>
        public void Add(T key, S value)
        {
            KeyPair<T,S> pair = new KeyPair<T,S>(key, value);
            base.Add(pair);
        }
    }

    /// <summary>
    /// Represents a KeyPair of string values (key and value)
    /// </summary>
    public class KeyPair : KeyPair<string,string> 
    { 
        public KeyPair(string key, string value) : base(key, value) { }
    }

    /// <summary>
    /// Represents a simple pair of Key and Value
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Key"/></typeparam>
    /// <typeparam name="S">The type of <see cref="Value"/></typeparam>
    public class KeyPair<T,S>
    {
        /// <summary>
        /// Create a KeyPair with the default values for <see cref="Key"/> 
        /// and <see cref="Value"/>
        /// </summary>
        public KeyPair()
            : this(default(T), default(S))
        {
        }

        /// <summary>
        /// Creates a new KeyPair
        /// </summary>
        /// <param name="key">The initial value of <see cref="Key"/></param>
        /// <param name="value">The initisl value of <see cref="Value"/></param>
        public KeyPair(T key, S value)
        {
            Key   = key;
            Value = value;
        }

        /// <summary>
        /// The key of the KeyPair
        /// </summary>
        public T Key
        {
            get;
            set;
        }

        /// <summary>
        /// The value of the KeyPair
        /// </summary>
        public S Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a string description of the KeyPair
        /// </summary>
        /// <returns>The pair's key and value separated by an '=' </returns>
        public override string ToString()
        {
            return string.Format("({0} = {1})", Key, Value);
        }

        /// <summary>
        /// Checks if this KeyPair equals another
        /// </summary>
        /// <param name="obj">The object to check the equality of</param>
        /// <returns>True if <paramref name="obj"/> is a <c>KeyPair</c> and its keys and values   
        /// are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is KeyPair<T, S>)
            {
                var other = (KeyPair<T, S>)obj;

                if (!System.Object.Equals(this.Key,   other.Key))   return false;
                if (!System.Object.Equals(this.Value, other.Value)) return false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a hashcode for this KeyPair
        /// </summary>
        /// <returns>The hashcodes of the <c>KeyPair</c>'s key and value XORed together</returns>
        public override int GetHashCode()
        {
            int keyHash   = Key   != null ? Key.GetHashCode() : 0;
            int valueHash = Value != null ? Value.GetHashCode() : 0;


            return keyHash ^ valueHash;
        }
    }
}
