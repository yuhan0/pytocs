﻿#region License
//  Copyright 2015 John Källén
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion

using Pytocs.CodeModel;
using Pytocs.Syntax;
using Pytocs.TypeInference;
using Pytocs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pytocs.Translate
{
    public class ConstructorGenerator : MethodGenerator
    {
        public ConstructorGenerator(FunctionDef f, List<Syntax.Parameter> args, CodeGenerator gen)
            : base(f, "", args, false, gen)
        {
        }

        protected override CodeMemberMethod Generate(CodeParameterDeclarationExpression[] parms)
        {
            var cons = gen.Constructor(parms, () => XlatConstructor(f.body));
            GenerateTupleParameterUnpackers(cons);
            LocalVariableGenerator.Generate(cons, globals);
            return cons;
        }

        private void XlatConstructor(SuiteStatement stmt)
        {
            if (stmt == null)
                return;

            var comments = StatementTranslator.ConvertFirstStringToComments(stmt.stmts);
            stmt.Accept(this.stmtXlat);
            if (gen.Scope.Count == 0)
                return;
            gen.Scope[0].ToString();
            var expStm = gen.Scope[0] as CodeExpressionStatement;
            if (expStm == null)
                return;
            var appl = expStm.Expression as CodeApplicationExpression;
            if (appl == null)
                return;
            var method = appl.Method as CodeFieldReferenceExpression;
            if (method == null || method.FieldName != "__init__")
                return;
            var ctor = (CodeConstructor) gen.CurrentMember;
            ctor.Comments.AddRange(comments);
            ctor.BaseConstructorArgs.AddRange(appl.Arguments.Skip(1));
            gen.Scope.RemoveAt(0);
        }

        protected override void GenerateDefaultArgMethod(
            CodeParameterDeclarationExpression[] argList,
            CodeExpression [] paramList)
        {
            var cons = gen.Constructor(argList, () => {});
            cons.ChainedConstructorArgs.AddRange(paramList);
        }
    }
}
