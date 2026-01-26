/*
 *   Copyright 2025 asdftowel
 *
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 */

using System;
using System.Text;

sealed class SpellNum {
    /// <summary>An array of English number names.</summary>
    static readonly string[] names = new[] {
        "",         "one",      "two",        "three",        "four",
        "five",     "six",      "seven",      "eight",        "nine",
        " ten",     " eleven",  " twelve",    " thirteen",    " fourteen",
        " fifteen", " sixteen", " seventeen", " eighteen",    " nineteen",
        " twenty",  " thirty",  " forty",     " fifty",       " sixty",
        " seventy", " eighty",  " ninety",    " hundred",     " thousand",
        " million", " billion", " trillion",  " quadrillion", " quintillion"
    };

    /// <summary>
    /// Extracts digits from an integer lesser than 1000. If any of the
    /// digits are not present, their value is set to 0.
    /// <example>
    /// For example:
    /// <code>
    /// const int number = 251;
    /// int hundreds, tens, units;
    /// (hundreds, tens, units) = SpellNum.ExtractHundred(number);
    /// Console.WriteLine(
    ///     $"{number} = {hundreds} * 100 + {tens} * 10 + {units}"
    /// );
    /// </code>
    /// prints <c>251 = 2 * 100 + 5 * 10 + 1</c>.
    /// </example>
    /// </summary>
    /// <param name="number">The number from which to extract digits.</param>
    /// <returns>
    /// A tuple containing every digit. Note that due to the name table's
    /// layout, if 10 &lt; <c>tens</c> &lt; 20, it returns both the second
    /// and the third digits in <c>tens</c>.
    /// </returns>
    static (int, int, int) ExtractHundred(in int number) {
        int tens, units, hundreds = Math.DivRem(number, 100, out tens);
        if (tens > 19) {
            tens = Math.DivRem(tens, 10, out units);
            tens += 18;
        } else if (tens < 10) {
            units = tens;
            tens = 0;
        } else {
            units = 0;
        }
        return (hundreds, tens, units);
    }

    /// <summary>
    /// Creates a representation of the provided number.
    /// <example>
    /// This code prints a textual representation of 25000:
    /// <code>
    /// Console.WriteLine(
    ///     BuildRepr(0, 2, 5, names[29]).TrimStart()
    /// );
    /// </code>
    /// Result: <c>twenty-five thousand</c>
    /// </example>
    /// </summary>
    /// <param name="hundreds">The leftmost digit.</param>
    /// <param name="tens">
    /// The middle digit if the middle and rightmost digits form a
    /// number greater than 20, their combination otherwise.
    /// </param>
    /// <param name="units">
    /// The rightmost digit if <c>tens</c> is 0 or greater than 1,
    /// otherwise 0.
    /// </param>
    /// <param name="magName">
    /// An optional order of magnitude, added to the end of the
    /// representation.
    /// </param>
    /// <returns>
    /// The textual representation of the number as a string.
    /// </returns>
    static string BuildRepr(
        in int hundreds,
        in int tens,
        in int units,
        in string magName
    ) {
        bool
            hasHundreds = hundreds != 0,
            hasTens = tens != 0,
            hasUnits = units != 0;
        var parts = new[] {
            String.Empty,
            names[hundreds],
            String.Empty,
            String.Empty,
            names[tens],
            String.Empty,
            names[units],
            magName 
        };
        if (hasHundreds) {
            parts[0] = " ";
            parts[2] = names[28];
            if (hasTens | hasUnits) {
                parts[3] = " and";
            }
        }
        if (hasUnits) {
            parts[5] = hasTens ? "-" : " ";
        }
        return String.Concat(parts);
    }

    /// <summary>
    /// Spells out an integer between -2 ** 63 and 2 ** 63 - 1 in English.
    /// <example>
    /// For example:
    /// <code>
    /// $ ./spellnum.exe
    /// Please provide an integer to spell.
    /// $ ./spellnum.exe hello
    /// Input string was not in a correct format.
    /// $ ./spellnum.exe 43110
    /// forty-three thousand one hundred and ten
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="args">Must be a single 64-bit integer.</param>
    /// <returns>0 on success, 1 otherwise.</returns>
    static int Main(string[] args) {
        var addOne = false;
        var spelling = new StringBuilder();
        long userNum, divisor;
        int magnitude, hundreds, tens, units;

        try {
            userNum = args.Length == 1 ? long.Parse(args[0]) : throw
                new ArgumentException("Please provide an integer to spell.");
        } catch (Exception e) when (
            e is FormatException   |
            e is ArgumentException |
            e is OverflowException
        ) {
            Console.WriteLine(e.Message);
            return 1;
        }

        if (userNum == 0) {
            spelling.Append("zero");
            goto ExitEarly;
        } else if (userNum < 0) {
            spelling.Append("minus");
            if (userNum == long.MinValue) {
                userNum = long.MaxValue;
                addOne = true;
            } else {
                userNum = Math.Abs(userNum);
            }
        }

        magnitude = (int)Math.Log10(userNum) / 3;
        switch (magnitude) {
            case 0:
                divisor = 0L;
                break;
            case 1:
                divisor = 1_000L;
                break;
            case 2:
                divisor = 1_000_000L;
                break;
            case 3:
                divisor = 1_000_000_000L;
                break;
            case 4:
                divisor = 1_000_000_000_000L;
                break;
            case 5:
                divisor = 1_000_000_000_000_000L;
                break;
            case 6:
                divisor = 1_000_000_000_000_000_000L;
                break;
            default:
                Console.WriteLine(
                    "This case should be unreachable because userNum" +
                    "is 64 bits wide."
                );
                return 1;
        }
        for (var i = 28 + magnitude; i != 28; --i, divisor /= 1000) {
            (hundreds, tens, units) = ExtractHundred(
                in (int)Math.DivRem(userNum, divisor, out userNum)
            );
            spelling.Append(
                BuildRepr(in hundreds, in tens, in units, in names[i])
            );
        }
        userNum += Convert.ToInt64(addOne);

        (hundreds, tens, units) = ExtractHundred(in (int)userNum);
        spelling.Append(BuildRepr(in hundreds, in tens, in units, in names[0]));
        ExitEarly:
        Console.WriteLine(spelling.ToString().TrimStart());
        return 0;
    }
}
