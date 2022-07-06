//===---------------------------------------------------===//
//                  Html.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using EightLeggedEssay.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace EightLeggedEssay.Cmdlet
{
    /// <summary>
    /// html检查器
    /// </summary>
    [OutputType(typeof(List<Html.HtmlError>))]
    [Cmdlet(VerbsDiagnostic.Test,"Html")]
    public class HtmlCheckCmdlet : PSCmdlet
    {

        public const string CallName = "Test-Html";

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 1)]
        public PSObject? InputObject { get; set; } = null;


        [Parameter(Mandatory = false, ValueFromPipeline = false, Position = 2)]
        public PSObject? Checker { get; set; } = null;


        protected override void ProcessRecord()
        {
            if(InputObject?.BaseObject == null)
            {
                WriteError(new ErrorRecord(
                    new ArgumentNullException(nameof(InputObject)),
                    null,
                    ErrorCategory.InvalidArgument,
                    InputObject));
                return;
            }

            var obj = InputObject.BaseObject;
            IHtmlChecker checker = 
                (IHtmlChecker?)Checker?.BaseObject 
                ?? new HtmlChecker();

            if (obj.GetType().IsAssignableTo(typeof(string)))
            {
                if (checker.TryGetError((string)obj, out List<Html.HtmlError> errors))
                {
                    WriteObject(errors);
                    return;
                }
                else
                {
                    return;
                }
            }
            else if (obj.GetType().IsAssignableTo(typeof(HtmlAgilityPack.HtmlDocument)))
            {
                if(checker.TryGetError((HtmlAgilityPack.HtmlDocument)obj, out List<Html.HtmlError> errors))
                {
                    WriteObject(errors);
                    return;
                }
                else
                {
                    return;
                }
            }
            else
            {
                WriteError(new ErrorRecord(
                    new InvalidCastException("Input Object is not a string or a HtmlAgilityPack.HtmlDocument"),
                    null,
                    ErrorCategory.InvalidArgument,
                    InputObject));
                return;
            }
        }

    }
}
