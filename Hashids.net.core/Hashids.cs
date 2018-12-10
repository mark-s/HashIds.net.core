using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hashids.net.core
{
    /// <summary>
    ///     Generate YouTube-like hashes from one or many numbers. Use hashids when you do not want to expose your database ids
    ///     to the user.
    /// </summary>
    public class Hashids : IHashids
    {

        private const int MIN_ALPHABET_LENGTH = 16;
        private const double SEP_DIV = 3.5;

        private string _alphabet;
        private string _guards;

        private Regex _guardsRegex;
        private static readonly Lazy<Regex> _hexSplitter = new Lazy<Regex>(() => new Regex(@"[\w\W]{1,12}", RegexOptions.Compiled));

        //  Creates the Regex in the first usage, speed up first use of non hex methods
        private static readonly Lazy<Regex> _hexValidator = new Lazy<Regex>(() => new Regex("^[0-9a-fA-F]+$", RegexOptions.Compiled));
        private readonly int _minHashLength;
        private readonly string _salt;
        private string _seps;
        private Regex _sepsRegex;

        public const string DEFAULT_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public const string DEFAULT_SEPS = "cfhistuCFHISTU";

        private const double GUARD_DIV = 12.0;

        /// <inheritdoc />
        /// <summary>
        ///     Instantiates a new Hashids with the default setup.
        /// </summary>
        public Hashids() : this(string.Empty, 0, DEFAULT_ALPHABET, DEFAULT_SEPS)
        {
        }

        /// <summary>
        ///     Instantiates a new Hashids en/de-coder.
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="minHashLength"></param>
        /// <param name="alphabet"></param>
        /// <param name="seps"></param>
        public Hashids(string salt = "", int minHashLength = 0, string alphabet = DEFAULT_ALPHABET, string seps = DEFAULT_SEPS)
        {
            if (string.IsNullOrWhiteSpace(alphabet))
                throw new ArgumentNullException(nameof(alphabet));

            if (minHashLength < 0)
                throw new ArgumentOutOfRangeException(nameof(minHashLength));

            _salt = salt;
            _alphabet = new string(alphabet.ToCharArray().Distinct().ToArray());
            _seps = seps;
            _minHashLength = minHashLength;

            if (_alphabet.Length < 16)
                throw new ArgumentException("alphabet must contain atleast 4 unique characters.", nameof(alphabet));

            SetupSeps();
            SetupGuards();
        }




        /// <summary>
        ///     Encodes the provided numbers into a hashed string
        /// </summary>
        /// <param name="numbers">the numbers to encode</param>
        /// <returns>the hashed string</returns>
        public virtual string Encode(params int[] numbers) 
            => numbers.Any(n => n < 0) ? string.Empty : GenerateHashFrom(numbers.Select(n => (long)n).ToArray());

        /// <summary>
        ///     Encodes the provided numbers into a hashed string
        /// </summary>
        /// <param name="numbers">the numbers to encode</param>
        /// <returns>the hashed string</returns>
        public virtual string Encode(IEnumerable<int> numbers)
            => Encode(numbers.ToArray());

        /// <summary>
        ///     Decodes the provided hash into
        /// </summary>
        /// <param name="hash">the hash</param>
        /// <exception cref="T:System.OverflowException">if the decoded number overflows integer</exception>
        /// <returns>the numbers</returns>
        public virtual int[] Decode(string hash)
            => GetNumbersFrom(hash).Select(n => (int)n).ToArray();

        /// <summary>
        ///     Encodes the provided hex string to a hashids hash.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public virtual string EncodeHex(string hex)
        {
            if (!_hexValidator.Value.IsMatch(hex))
                return string.Empty;

            var matches = _hexSplitter.Value.Matches(hex);
            var numbers = new List<long>(matches.Count);

            foreach (Match match in matches)
            {
                var number = Convert.ToInt64(string.Concat("1", match.Value), 16);
                numbers.Add(number);
            }

            return EncodeLong(numbers.ToArray());
        }

        /// <summary>
        ///     Decodes the provided hash into a hex-string
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public virtual string DecodeHex(string hash)
        {
            var sb = new StringBuilder();
            var numbers = DecodeLong(hash);

            foreach (var number in numbers)
                sb.Append($"{number:X}".Substring(1));

            return sb.ToString();
        }

        /// <summary>
        ///     Decodes the provided hashed string into an array of longs
        /// </summary>
        /// <param name="hash">the hashed string</param>
        /// <returns>the numbers</returns>
        public long[] DecodeLong(string hash)
            => GetNumbersFrom(hash);

        /// <summary>
        ///     Encodes the provided longs to a hashed string
        /// </summary>
        /// <param name="numbers">the numbers</param>
        /// <returns>the hashed string</returns>
        public string EncodeLong(params long[] numbers)
            => numbers.Any(n => n < 0) ? string.Empty : GenerateHashFrom(numbers);

        /// <summary>
        ///     Encodes the provided longs to a hashed string
        /// </summary>
        /// <param name="numbers">the numbers</param>
        /// <returns>the hashed string</returns>
        public string EncodeLong(IEnumerable<long> numbers)
            => EncodeLong(numbers.ToArray());


        /// <summary>
        /// </summary>
        private void SetupGuards()
        {
            var guardCount = (int)Math.Ceiling(_alphabet.Length / GUARD_DIV);

            if (_alphabet.Length < 3)
            {
                _guards = _seps.Substring(0, guardCount);
                _seps = _seps.Substring(guardCount);
            }
            else
            {
                _guards = _alphabet.Substring(0, guardCount);
                _alphabet = _alphabet.Substring(guardCount);
            }

            _guardsRegex = new Regex(string.Concat("[", _guards, "]"), RegexOptions.Compiled);
        }

        /// <summary>
        /// </summary>
        private void SetupSeps()
        {
            // seps should contain only characters present in alphabet; 
            _seps = new string(_seps.ToCharArray().Intersect(_alphabet.ToCharArray()).ToArray());

            // alphabet should not contain seps.
            _alphabet = new string(_alphabet.ToCharArray().Except(_seps.ToCharArray()).ToArray());

            _seps = ConsistentShuffle(_seps, _salt);

            if (_seps.Length == 0 || _alphabet.Length / _seps.Length > SEP_DIV)
            {
                var sepsLength = (int)Math.Ceiling(_alphabet.Length / SEP_DIV);
                if (sepsLength == 1)
                    sepsLength = 2;

                if (sepsLength > _seps.Length)
                {
                    var diff = sepsLength - _seps.Length;
                    _seps += _alphabet.Substring(0, diff);
                    _alphabet = _alphabet.Substring(diff);
                }

                else
                {
                    _seps = _seps.Substring(0, sepsLength);
                }
            }

            _sepsRegex = new Regex(string.Concat("[", _seps, "]"), RegexOptions.Compiled);

            _alphabet = ConsistentShuffle(_alphabet, _salt);
        }

        /// <summary>
        /// </summary>
        /// <param name="alphabet"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        private static string ConsistentShuffle(string alphabet, string salt)
        {
            if (string.IsNullOrWhiteSpace(salt))
                return alphabet;

            var letters = alphabet.ToCharArray();
            for (int i = letters.Length - 1, v = 0, p = 0; i > 0; i--, v++)
            {
                v %= salt.Length;
                int n;
                p += n = salt[v];
                var j = (n + v + p) % i;
                // swap characters at positions i and j
                var temp = letters[j];
                letters[j] = letters[i];
                letters[i] = temp;
            }

            return new string(letters);
        }

        /// <summary>
        ///     Internal function that does the work of creating the hash
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        private string GenerateHashFrom(long[] numbers)
        {
            if (numbers == null || numbers.Length == 0)
                return string.Empty;

            var ret = new StringBuilder();
            var alphabet = _alphabet;

            long numbersHashInt = 0;
            for (var i = 0; i < numbers.Length; i++)
                numbersHashInt += (int)(numbers[i] % (i + 100));

            var lottery = alphabet[(int)(numbersHashInt % alphabet.Length)];
            ret.Append(lottery.ToString());

            for (var i = 0; i < numbers.Length; i++)
            {
                var number = numbers[i];
                var buffer = lottery + _salt + alphabet;

                alphabet = ConsistentShuffle(alphabet, buffer.Substring(0, alphabet.Length));
                var last = Hash(number, alphabet);

                ret.Append(last);

                if (i + 1 < numbers.Length)
                {
                    number %= last[0] + i;
                    var sepsIndex = (int)number % _seps.Length;

                    ret.Append(_seps[sepsIndex]);
                }
            }

            if (ret.Length < _minHashLength)
            {
                var guardIndex = (int)(numbersHashInt + ret[0]) % _guards.Length;
                var guard = _guards[guardIndex];

                ret.Insert(0, guard);

                if (ret.Length < _minHashLength)
                {
                    guardIndex = (int)(numbersHashInt + ret[2]) % _guards.Length;
                    guard = _guards[guardIndex];

                    ret.Append(guard);
                }
            }

            var halfLength = alphabet.Length / 2;
            while (ret.Length < _minHashLength)
            {
                alphabet = ConsistentShuffle(alphabet, alphabet);
                ret.Insert(0, alphabet.Substring(halfLength));
                ret.Append(alphabet.Substring(0, halfLength));

                var excess = ret.Length - _minHashLength;
                if (excess > 0)
                {
                    ret.Remove(0, excess / 2);
                    ret.Remove(_minHashLength, ret.Length - _minHashLength);
                }
            }

            return ret.ToString();
        }

        private long[] GetNumbersFrom(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return new long[0];

            var alphabet = new string(_alphabet.ToCharArray());
            var ret = new List<long>();
            var i = 0;

            var hashBreakdown = _guardsRegex.Replace(hash, " ");
            var hashArray = hashBreakdown.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (hashArray.Length == 3 || hashArray.Length == 2)
                i = 1;

            hashBreakdown = hashArray[i];
            if (hashBreakdown[0] != default(char))
            {
                var lottery = hashBreakdown[0];
                hashBreakdown = hashBreakdown.Substring(1);

                hashBreakdown = _sepsRegex.Replace(hashBreakdown, " ");
                hashArray = hashBreakdown.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (var j = 0; j < hashArray.Length; j++)
                {
                    var subHash = hashArray[j];
                    var buffer = lottery + _salt + alphabet;

                    alphabet = ConsistentShuffle(alphabet, buffer.Substring(0, alphabet.Length));
                    ret.Add(Unhash(subHash, alphabet));
                }

                if (EncodeLong(ret.ToArray()) != hash)
                    ret.Clear();
            }

            return ret.ToArray();
        }

        private static long Unhash(string input, string alphabet)
        {
            long number = 0;
            var alphabetLength = new System.Numerics.BigInteger(alphabet.Length);

            for (var i = 0; i < input.Length; i++)
            {
                var pos = (long)alphabet.IndexOf(input[i]);
                number += (pos * (long)System.Numerics.BigInteger.Pow(alphabetLength, input.Length - i - 1));
            }

            return number;
        }

        private static string Hash(long input, string alphabet)
        {
            var hash = new StringBuilder();

            do
            {
                hash.Insert(0, alphabet[(int)(input % alphabet.Length)]);
                input = input / alphabet.Length;
            } while (input > 0);

            return hash.ToString();
        }


    }
}