using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Hashids.net.core.Tests
{
    public class HashidsIssuesTests
    {
        [Fact]
        private void Issue_8_should_not_throw_out_of_range_exception()
        {
            var hashids = new Hashids("janottaa", 6);
            Should.NotThrow(() => hashids.Decode("NgAzADEANAA="));
        }

        // This issue came from downcasting to int at the wrong place,
        // seems to happen when you are encoding A LOT of longs at the same time.
        // see if it is possible to make this a faster test (or remove it since it is unlikely that it will reapper).
        [Fact]
        private void Issue_12_should_not_throw_out_of_range_exception()
        {
            var hash = new Hashids("zXZVFf2N38uV");
            var longs = new List<long>();
            var rand = new Random();
            var valueBuffer = new byte[8];
            for (var i = 0; i < 100000; i++)
            {
                rand.NextBytes(valueBuffer);
                var randLong = BitConverter.ToInt64(valueBuffer, 0);
                longs.Add(Math.Abs(randLong));
            }

            var encoded = hash.EncodeLong(longs);
            var decoded = hash.DecodeLong(encoded);
            decoded.ShouldBe(longs.ToArray());
        }

        [Fact]
        private void Issue_14_it_should_decode_encode_hex_correctly()
        {
            var hashids = new Hashids("this is my salt");
            var encoded = hashids.EncodeHex("DEADBEEF");
            encoded.ShouldBe("kRNrpKlJ");

            var decoded = hashids.DecodeHex(encoded);
            decoded.ShouldBe("DEADBEEF");

            var encoded2 = hashids.EncodeHex("1234567890ABCDEF");
            var decoded2 = hashids.DecodeHex(encoded2);
            decoded2.ShouldBe("1234567890ABCDEF");
        }

        [Fact]
        private void Issue_18_it_should_return_empty_string_if_negative_numbers()
        {
            var hashids = new Hashids("this is my salt");
            hashids.Encode(1, 4, 5, -3).ShouldBe(string.Empty);
            hashids.EncodeLong(4, 5, 2, -4).ShouldBe(string.Empty);
        }

        [Fact]
        private void Issue_15_it_should_return_emtpy_array_when_decoding_characters_missing_in_alphabet()
        {
            var hashids = new Hashids(salt: "Salty stuff", alphabet: "qwerty1234!¤%&/()=", seps: "1234");
            var numbers = hashids.Decode("abcd");
            numbers.Length.ShouldBe(0);

            var hashids2 = new Hashids();
            hashids2.Decode("13-37").Length.ShouldBe(0);
            hashids2.DecodeLong("32323kldffd!").Length.ShouldBe(0);

            var hashids3 = new Hashids(alphabet: "1234567890;:_!#¤%&/()=", seps: "!#¤%&/()=");
            hashids3.Decode("asdfb").Length.ShouldBe(0);
            hashids3.DecodeLong("asdfgfdgdfgkj").Length.ShouldBe(0);
        }

        //[Fact(Skip = "Might not be a good  thing")]
        //private void Issue21_It_shouldnot_throw_IndexOutOfRangeException_when_decoding_string_with_guard_chars()
        //{
        //    // this generates the following guards: rKEa
        //    var hashids = new Hashids("please fix me <3", 15);

        //    // passing any of the chars defined in the guard may throw and exception
        //    Should.NotThrow(() => hashids.Decode("a"));
        
        //}


        [Fact]
        private void Issue23_It_should_Encode_hash()
        {
            var hashId = new Hashids(salt: "0Q6wKupsoahWD5le", alphabet: "abcdefghijklmnopqrstuvwxyz1234567890!", seps: "cfhistu");
            var source = new long[] { 35887507618889472L, 30720L, Int64.MaxValue };
            var encoded = hashId.EncodeLong(source);

            var result = hashId.DecodeLong(encoded);

            result.ShouldBe(source);
        }

    }
}
