using PasswordManagerAppResourceServer.Handlers;
using PasswordManagerAppResourceServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PasswordManagerAppTests
{
    public class AesHelperTests
    {
           
        public AesHelperTests()
        {
            
        }
        [Theory]
        [MemberData(nameof(RandomStringAndKeys))]
        public void EncryptAES_ValidCall(string plainText, byte[] key)
        {

            
            
            
            var firstCallValue = AESHelper.EncryptAES(plainText, key);
            var secondCallValue = AESHelper.EncryptAES(plainText, key);
             
            
            

            Assert.NotNull(firstCallValue);
            Assert.NotNull(secondCallValue);
            Assert.NotEqual(firstCallValue, secondCallValue);
            Assert.Equal(firstCallValue.Length, secondCallValue.Length);
            
            

        }
        [Theory]
        [MemberData(nameof(RandomStringAndKeys))]
        public void EncryptAES_CallShouldGenerateDifferentCypherTextForTheSamePlainText(string plainText, byte[] key)
        {


            for (int i = 0; i < 5; i++)
            {
                var firstCallValue = AESHelper.EncryptAES(plainText, key);
                var secondCallValue = AESHelper.EncryptAES(plainText, key);
                
                Assert.NotEqual(firstCallValue, secondCallValue);
                Assert.Equal(firstCallValue.Length, secondCallValue.Length);
            }

            




            
            



        }
        [Theory]
        [MemberData(nameof(RandomStringAndKeys))]
        public void DecryptAES_ValidCallShouldGenerateTheSamePlainText(string plainText, byte[] key)
        {

            var expected = plainText;
            
                
            var actual = AESHelper.DecryptAES(AESHelper.EncryptAES(plainText, key), key);
                

                
            Assert.Equal(expected.Length, actual.Length);
            Assert.Equal(expected[4], actual[4]);
            Assert.Equal(expected, actual);
        }
        [Theory]
        [MemberData(nameof(RandomInvalidKeys))]
        public void EncryptAES_InvalidKeysShouldThrowError(string plainText, byte[] key)
        {

            


            Action actionEncrypt = () => AESHelper.EncryptAES(plainText, key);
            Action actionDecrypt = () => AESHelper.DecryptAES(plainText, key);



            Assert.ThrowsAny<Exception>(actionEncrypt);
            Assert.ThrowsAny<Exception>(actionDecrypt);
            
        }



















        private static string GetRandomString(int length)
        {
            var random = new Random();
        
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        
        }
        private static byte[] GetRandomByte(int length)
        {
            var rnd = new Random();
            Byte[] b = new Byte[length];
             rnd.NextBytes(b);
            return b;
        }

        public static IEnumerable<object[]> RandomStringAndKeys =>
        new List<object[]>
        {
            new object[] { GetRandomString(8), GetRandomByte(32) },
            new object[] { GetRandomString(55), GetRandomByte(32)},
            new object[] { GetRandomString(14), GetRandomByte(32)},
            new object[] { GetRandomString(156), GetRandomByte(32)},
            new object[] { GetRandomString(561), GetRandomByte(32)},
            
            
        };
        public static IEnumerable<object[]> RandomInvalidKeys =>
        new List<object[]>
        {
            new object[] { GetRandomString(7), GetRandomByte(31) },
            new object[] { GetRandomString(11), GetRandomByte(30)},
            new object[] { GetRandomString(48), GetRandomByte(2)},
            new object[] { GetRandomString(55), GetRandomByte(33)},
            new object[] { GetRandomString(118), GetRandomByte(12)},


        };
    }
}
