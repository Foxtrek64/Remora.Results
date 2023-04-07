//
//  AggregateResult.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Remora.Results
{
    /// <summary>
    /// A <see cref="Result"/> type which represents the results of an aggregate set of operations.
    /// </summary>
    public readonly struct AggregateResult : IResult
    {
        /// <summary>
        /// Gets a value indicating whether all results contained in this result were successful.
        /// </summary>
        [MemberNotNullWhen(false, nameof(Error))]
        public bool IsSuccess => this.Error is null;

        /// <summary>
        /// Gets a collection of all results contained within this <see cref="AggregateResult"/>.
        /// </summary>
        public IReadOnlyCollection<IResult> Results { get; }

        /// <summary>
        /// Gets a collection of the results which were unsuccessful.
        /// </summary>
        public IReadOnlyCollection<IResult> SuccessfulResults { get; }

        /// <summary>
        /// Gets a collection of the results which were unsuccessful.
        /// </summary>
        public IReadOnlyCollection<IResult> FailedResults { get; }

        /// <inheritdoc/>
        public IResultError? Error { get; }

        /// <inheritdoc />
        /// <remarks>
        ///     Unused. Always returns null.
        /// </remarks>
        [Obsolete("Unused. Always returns null.")]
        [EditorBrowsable(EditorBrowsableState.Never)] // Hide this property from intellisense. No reason they should try to call it.
        public IResult? Inner => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateResult"/> struct.
        /// </summary>
        /// <param name="results">A set of result values.</param>
        public AggregateResult(IReadOnlyCollection<IResult> results)
        {
            Results = results;

            var successes = new List<IResult>();
            var failures = new List<IResult>();

            foreach (var result in results)
            {
                if (result.IsSuccess)
                {
                    successes.Add(result);
                }
                else
                {
                    failures.Add(result);
                }
            }

            SuccessfulResults = successes.AsReadOnly();
            FailedResults = failures.AsReadOnly();

            this.Error = FailedResults.Any()
                ? new AggregateError(FailedResults, "One or more errors occurred: ")
                : null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (this.IsSuccess)
            {
                return "All operations completed successfully!";
            }

            var sb = new StringBuilder(Error.Message);

            var resultNumber = 0;
            foreach (var result in Results)
            {
                if (result.IsSuccess)
                {
                    sb.AppendLine($"Result {resultNumber++}: Operation Completed Successfully!");
                    continue;
                }

                sb.AppendLine($"Result {resultNumber++}: Failed!");

                var error = result.Error!;
                sb.AppendLine($"\tOperation returned an error of type '{error.GetType().Name}'. Message: \"{error.Message}\"");

                if (error is AggregateError ae)
                {
                    sb.Append(ae.ToString());
                }
            }

            return sb.ToString();
        }

        /*
        internal static string FormatAggregateError(AggregateError aggregateError)
        {

        }
        */
    }
}
