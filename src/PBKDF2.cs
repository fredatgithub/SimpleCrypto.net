﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCrypto
{
    public class PBKDF2 : ICryptoService
    {
        public PBKDF2()
        {
            //Set default salt size and hashiterations
            HashIterations = 50;
            SaltSize = 16;
        }

        public int HashIterations
        { get; set; }

        public int SaltSize
        { get; set; }

        public string PlainText
        { get; set; }

        public string HashedText 
        { get; private set; }

        public string Salt 
        { get; private set; }

        public string Compute()
        {
            if (string.IsNullOrEmpty(PlainText)) throw new InvalidOperationException("PlainText cannot be empty");

            if(SaltSize < 1) throw new InvalidOperationException(string.Format("Cannot generate a salt of size {0}, use a value greater than 1, recommended: 16", SaltSize));
            if (HashIterations < 1) throw new InvalidOperationException("HashIterations cannot be less than 1, recommended: 50");



            //convert the plain text into a byte array
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(PlainText);

            //convert the salt into a byte array
            byte[] saltBytes = Encoding.UTF8.GetBytes(Salt);

            // Allocate array, which will hold plain text and salt.
            byte[] plainTextWithSaltBytes =
                    new byte[plainTextBytes.Length + saltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int a = 0; a < plainTextBytes.Length; a++)
                plainTextWithSaltBytes[a] = plainTextBytes[a];

            // Append salt bytes to the resulting array.
            for (int a = 0; a < saltBytes.Length; a++)
                plainTextWithSaltBytes[plainTextBytes.Length + a] = saltBytes[a];


            using (var pbkdf2 = new Rfc2898DeriveBytes(PlainText, saltBytes, HashIterations))
            {
                var key = pbkdf2.GetBytes(64);
                HashedText = Convert.ToBase64String(key);
            }

            return HashedText;
        }

        public string Compute(string textToHash)
        {
            PlainText = textToHash;
            //generate the salt
            generateSalt();
            //compute the hash
            Compute();
            return HashedText;
        }


        public string Compute(string textToHash, int saltSize, int hashIterations)
        {
            PlainText = textToHash;
            HashIterations = hashIterations;
            SaltSize = saltSize;
            //generate the salt
            generateSalt();
            //compute the hash
            Compute();
            return HashedText;
        }

        public string Compute(string textToHash, string salt)
        {
            PlainText = textToHash;
            Salt = salt;
            //expand the salt
            expandSalt();
            Compute();
            return HashedText;
        }

        private void generateSalt()
        {
            var rand = RandomNumberGenerator.Create();

            var ret = new byte[SaltSize];

            rand.GetBytes(ret);

            //assign the generated salt in the format of {iterations}.{salt}
            Salt = string.Format("{0}.{1}",HashIterations, Convert.ToBase64String(ret));
        }

        private void expandSalt()
        {
            //TODO: Add appropiate exceptions

            //get the position of the . that splits the string
            var i = Salt.IndexOf('.');
            
            HashIterations = int.Parse(Salt.Substring(0, i), System.Globalization.NumberStyles.Number);
            
        }
    }
}
