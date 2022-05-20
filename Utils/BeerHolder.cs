using System;
using System.Collections.Generic;
using Godot;
using Serilog;

namespace OpenScadGraphEditor.Utils
{
    /// <summary>
    /// This is a workaround for avoiding getting data into the realm of Godot that is not serializable by
    /// Godot. Instead of this, you can give this data to the BeerHolder which will hold your beer for a while
    /// and give you a serializable object back. You can later exchange the object back for your beer. The object
    /// will also automatically release any reference of your beer if the serializable object goes out of scope.
    /// </summary>
    public static class BeerHolder
    {
        private static readonly Dictionary<string, object> Beers = new Dictionary<string, object>();

        public static Reference HoldMyBeer(this object beer)
        {
            var beerId = Guid.NewGuid().ToString();
            Beers[beerId] = beer;
            var reference = new BeerReference();
            reference.Key = beerId;
            Log.Debug("Holding your beer {Beer}", beerId);
            return reference;
        }

        public static bool TryGetBeer<T>(this Reference holder, out T result)
        {
            if (holder is BeerReference beerReference && Beers.TryGetValue(beerReference.Key, out var beer) && beer is T tBeer)
            {
                result = tBeer;
                return true;
            }

            result = default;
            return false;
        }

        private static void YouCanKeepThisBeer(string key)
        {
            if (Beers.ContainsKey(key))
            {
                Beers.Remove(key);
                Log.Debug("Beer {Beer} is no more", key);
            }
        }


        private class BeerReference : Reference
        {
            public string Key;

            public override void _Notification(int what)
            {
                if (what == NotificationPredelete)
                {
                    YouCanKeepThisBeer(Key);
                }
            }
        }
    }
}