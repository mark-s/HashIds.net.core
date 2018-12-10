using System;
using Shouldly;
using Xunit;

namespace Hashids.net.core.Tests
{
    public class HashidsTest
    {
        private readonly Hashids _hashids;
        private const string SALT = "this is my salt";
        private const string DEFAULT_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private const string DEFAULT_SEPS = "cfhistuCFHISTU";

        public HashidsTest()
        {
            _hashids = new Hashids(SALT);
        }

        [Fact]
        private void It_has_correct_default_alphabet()
        {
            Hashids.DEFAULT_ALPHABET.ShouldBe(DEFAULT_ALPHABET);
        }

        [Fact]
        private void It_has_correct_default_separators()
        {
            Hashids.DEFAULT_SEPS.ShouldBe(DEFAULT_SEPS);
        }

        [Fact]
        private void It_has_a_default_salt()
        {
            new Hashids().Encode(1, 2, 3).ShouldBe("o2fXhV");
        }

        [Fact]
        private void It_encodes_a_single_number()
        {
            _hashids.Encode(1).ShouldBe("NV");
            _hashids.Encode(22).ShouldBe("K4");
            _hashids.Encode(333).ShouldBe("OqM");
            _hashids.Encode(9999).ShouldBe("kQVg");
            _hashids.Encode(123000).ShouldBe("58LzD");
            _hashids.Encode(456000000).ShouldBe("5gn6mQP");
            _hashids.Encode(987654321).ShouldBe("oyjYvry");

        }

        [Fact]
        private void It_encodes_a_single_long()
        {
            _hashids.EncodeLong(1L).ShouldBe("NV");
            _hashids.EncodeLong(2147483648L).ShouldBe("21OjjRK");
            _hashids.EncodeLong(4294967296L).ShouldBe("D54yen6");

            _hashids.EncodeLong(666555444333222L).ShouldBe("KVO9yy1oO5j");
            _hashids.EncodeLong(12345678901112L).ShouldBe("4bNP1L26r");
            _hashids.EncodeLong(Int64.MaxValue).ShouldBe("jvNx4BjM5KYjv");
        }

        [Fact]
        private void It_encodes_a_list_of_numbers()
        {
            _hashids.Encode(1, 2, 3).ShouldBe("laHquq");
            _hashids.Encode(2, 4, 6).ShouldBe("44uotN");
            _hashids.Encode(99, 25).ShouldBe("97Jun");

            _hashids.Encode(1337, 42, 314).
              ShouldBe("7xKhrUxm");

            _hashids.Encode(683, 94108, 123, 5).
              ShouldBe("aBMswoO2UB3Sj");

            _hashids.Encode(547, 31, 241271, 311, 31397, 1129, 71129).
              ShouldBe("3RoSDhelEyhxRsyWpCx5t1ZK");

            _hashids.Encode(21979508, 35563591, 57543099, 93106690, 150649789).
              ShouldBe("p2xkL3CK33JjcrrZ8vsw4YRZueZX9k");
        }

        [Fact]
        private void It_encodes_a_list_of_longs()
        {
            _hashids.EncodeLong(666555444333222L, 12345678901112L).ShouldBe("mPVbjj7yVMzCJL215n69");
        }

        [Fact]
        private void It_returns_an_empty_string_if_no_numbers()
        {
            _hashids.Encode().ShouldBe(string.Empty);
        }

        [Fact]
        private void It_can_encodes_to_a_minimum_length()
        {
            var h = new Hashids(SALT, 18);
            h.Encode(1).ShouldBe("aJEDngB0NV05ev1WwP");

            h.Encode(4140, 21147, 115975, 678570, 4213597, 27644437).
                ShouldBe("pLMlCWnJSXr1BSpKgqUwbJ7oimr7l6");
        }

        [Fact]
        private void It_can_encode_with_a_custom_alphabet()
        {
            var h = new Hashids(SALT, 0, "ABCDEFGhijklmn34567890-:");
            h.Encode(1, 2, 3, 4, 5).ShouldBe("6nhmFDikA0");
        }

        [Fact]
        private void It_does_not_produce_repeating_patterns_for_identical_numbers()
        {
            _hashids.Encode(5, 5, 5, 5).ShouldBe("1Wc8cwcE");
        }

        [Fact]
        private void It_does_not_produce_repeating_patterns_for_incremented_numbers()
        {
            _hashids.Encode(1, 2, 3, 4, 5, 6, 7, 8, 9, 10).
                ShouldBe("kRHnurhptKcjIDTWC3sx");
        }

        [Fact]
        private void It_does_not_produce_similarities_between_incrementing_number_hashes()
        {
            _hashids.Encode(1).ShouldBe("NV");
            _hashids.Encode(2).ShouldBe("6m");
            _hashids.Encode(3).ShouldBe("yD");
            _hashids.Encode(4).ShouldBe("2l");
            _hashids.Encode(5).ShouldBe("rD");
        }

        [Fact]
        private void It_encode_hex_string()
        {
            _hashids.EncodeHex("FA").ShouldBe("lzY");
            _hashids.EncodeHex("26dd").ShouldBe("MemE");
            _hashids.EncodeHex("FF1A").ShouldBe("eBMrb");
            _hashids.EncodeHex("12abC").ShouldBe("D9NPE");
            _hashids.EncodeHex("185b0").ShouldBe("9OyNW");
            _hashids.EncodeHex("17b8d").ShouldBe("MRWNE");
            _hashids.EncodeHex("1d7f21dd38").ShouldBe("4o6Z7KqxE");
            _hashids.EncodeHex("20015111d").ShouldBe("ooweQVNB");
        }

        [Fact]
        private void It_returns_an_empty_string_if_passed_non_hex_string()
        {
            _hashids.EncodeHex("XYZ123").ShouldBe(string.Empty);
        }

        [Fact]
        private void It_decodes_an_encoded_number()
        {
            _hashids.Decode("NkK9").ShouldBe(new[] { 12345 });
            _hashids.Decode("5O8yp5P").ShouldBe(new[] { 666555444 });

            _hashids.Decode("Wzo").ShouldBe(new[] { 1337 });
            _hashids.Decode("DbE").ShouldBe(new[] { 808 });
            _hashids.Decode("yj8").ShouldBe(new[] { 303 });
        }

        [Fact]
        private void It_decodes_an_encoded_long()
        {
            _hashids.DecodeLong("NV").ShouldBe(new[] { 1L });
            _hashids.DecodeLong("21OjjRK").ShouldBe(new[] { 2147483648L });
            _hashids.DecodeLong("D54yen6").ShouldBe(new[] { 4294967296L });

            _hashids.DecodeLong("KVO9yy1oO5j").ShouldBe(new[] { 666555444333222L });
            _hashids.DecodeLong("4bNP1L26r").ShouldBe(new[] { 12345678901112L });
            _hashids.DecodeLong("jvNx4BjM5KYjv").ShouldBe(new[] { Int64.MaxValue });
        }

        [Fact]
        private void It_decodes_a_list_of_encoded_numbers()
        {
            _hashids.Decode("1gRYUwKxBgiVuX").ShouldBe(new[] { 66655, 5444333, 2, 22 });
            _hashids.Decode("aBMswoO2UB3Sj").ShouldBe(new[] { 683, 94108, 123, 5 });

            _hashids.Decode("jYhp").ShouldBe(new[] { 3, 4 });
            _hashids.Decode("k9Ib").ShouldBe(new[] { 6, 5 });

            _hashids.Decode("EMhN").ShouldBe(new[] { 31, 41 });
            _hashids.Decode("glSgV").ShouldBe(new[] { 13, 89 });
        }

        [Fact]
        private void It_decodes_a_list_of_longs()
        {
            _hashids.DecodeLong("mPVbjj7yVMzCJL215n69").ShouldBe(new[] { 666555444333222L, 12345678901112L });
        }

        [Fact]
        private void It_does_not_decode_with_a_different_salt()
        {
            var peppers = new Hashids("this is my pepper");
            _hashids.Decode("NkK9").ShouldBe(new[] { 12345 });
            peppers.Decode("NkK9").ShouldBe(new int[0]);
        }

        [Fact]
        private void It_can_decode_from_a_hash_with_a_minimum_length()
        {
            var h = new Hashids(SALT, 8);
            h.Decode("gB0NV05e").ShouldBe(new[] { 1 });
            h.Decode("mxi8XH87").ShouldBe(new[] { 25, 100, 950 });
            h.Decode("KQcmkIW8hX").ShouldBe(new[] { 5, 200, 195, 1 });
        }

        [Fact]
        private void It_decode_an_encoded_number()
        {
            _hashids.DecodeHex("lzY").ShouldBe("FA");
            _hashids.DecodeHex("eBMrb").ShouldBe("FF1A");
            _hashids.DecodeHex("D9NPE").ShouldBe("12ABC");
        }

        [Fact]
        private void It_raises_an_argument_null_exception_when_alphabet_is_null()
        {
            Action invocation = () => new Hashids(alphabet: null);
            invocation.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void It_raises_an_argument_null_exception_if_alphabet_contains_less_than_4_unique_characters()
        {
            Action invocation = () => new Hashids(alphabet: "aadsss");
            invocation.ShouldThrow<ArgumentException>();
        }

        [Fact]
        private void It_encodes_and_decodes_numbers_starting_with_0()
        {
            var hash = _hashids.Encode(0, 1, 2);
            _hashids.Decode(hash).ShouldBe(new[] { 0, 1, 2 });
        }

        [Fact]
        private void It_encodes_and_decodes_numbers_ending_with_0()
        {
            var hash = _hashids.Encode(1, 2, 0);
            _hashids.Decode(hash).ShouldBe(new[] { 1, 2, 0 });
        }

        //[Fact]
        //private void our_public_methods_can_be_mocked()
        //{
        //    var mock = new Mock<Hashids>();
        //    mock.Setup(hashids => hashids.Encode(It.IsAny<int[]>())).Returns("It works");
        //    mock.Object.Encode(new[] { 1 }).ShouldBe("It works");
        //}
    }
}
