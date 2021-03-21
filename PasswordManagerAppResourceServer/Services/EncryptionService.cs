using PasswordManagerAppResourceServer.Handlers;
using PasswordManagerAppResourceServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManagerAppResourceServer.Services
{

    public class EncryptionService:IEncryptionService
    {
        

       
        
        public string Encrypt(string password, string data) 
        {
            using (Aes myAes = Aes.Create())
            {
                myAes.Key = dataToSHA256(password);
                myAes.Mode = CipherMode.CBC;
                myAes.Padding = PaddingMode.PKCS7;
                
                return AESHelper.EncryptAES(data, myAes.Key);
            }   
        }
         public string Decrypt(string password, string encData)
        {
            using (Aes myAes = Aes.Create())
            {
                myAes.Key = dataToSHA256(password);
                myAes.Mode = CipherMode.CBC;
                myAes.Padding = PaddingMode.PKCS7;

                return AESHelper.DecryptAES(encData, myAes.Key);
            }   
        }
        
        private string ToBase64String(byte[] data) => Convert.ToBase64String(data);
        private byte[] dataToSHA256(string data)
        {
            SHA256 mysha256 = SHA256.Create();
            return mysha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        }


    }

    public interface IEncryptionService
    {
        string Encrypt(string password, string data);
        string Decrypt(string password, string encData);
    }
}

