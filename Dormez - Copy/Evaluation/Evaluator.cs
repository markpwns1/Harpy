using System;
using System.Collections.Generic;
using System.Linq;
using Harpy;

namespace Harpy.Evaluation
{
    public class Evaluator
    {
        private Parser p;
        public List<List<Operation>> precedences = new List<List<Operation>>();
        
        int loopid = 0;
        int ifid = 0;

        public UserFunction currentContext = null;
        
        Value binaryOp(Value left, Value right, Value.BatchType type, string op)
        {
            if (left.type == Value.BatchType.Indeterminate || right.type == Value.BatchType.Indeterminate)
                throw p.Exception("Cannot perform operation on untyped values. Please use type()");

            if (Value.TypeConflict(left.type, type))
            {
                throw new Exception("Expected " + type.ToString() + " on the lefthand side, not " + left.type.ToString());
            }

            if (Value.TypeConflict(right.type, type))
            {
                throw new Exception("Expected " + type.ToString() + " on the lefthand side, not " + right.type.ToString());
            }

            return new Value(type, left + op + right);
        }

        Value binaryOp(Value left, Value right, Value.BatchType inputType, Value.BatchType returnType, string op)
        {
            if (Value.TypeConflict(left.type, inputType))
            {
                throw new Exception("Expected " + inputType.ToString() + " on the lefthand side, not " + left.type.ToString());
            }

            if (Value.TypeConflict(right.type, inputType))
            {
                throw new Exception("Expected " + inputType.ToString() + " on the lefthand side, not " + right.type.ToString());
            }

            return new Value(returnType, left + " " + op + " " + right);
        }

        public void RegisterFunction()
        {
            bool inline = false;
            if(p.CurrentToken == "inline")
            {
                p.Eat();
                inline = true;
            }

            string name = p.GetIdentifier();

            List<string> argNames = new List<string>();
            List<Value.BatchType> argTypes = new List<Value.BatchType>();

            p.Eat("l bracket");
            while (p.CurrentToken != "r bracket")
            {
                argNames.Add(p.GetIdentifier());
                p.Eat("colon");
                argTypes.Add(Value.ParseType(p.GetIdentifier()));

                if (p.CurrentToken != "r bracket")
                    p.Eat("comma");
            }
            p.Eat();

            Value.BatchType returnType = Value.BatchType.Void;
            if (p.CurrentToken == "colon")
            {
                p.Eat("colon");
                returnType = Value.ParseType(p.GetIdentifier());
            }

            var func = UserFunctions.Register(name, argTypes, returnType, inline);
            func.argumentNames = argNames;
        }

        public void DoIncludes()
        {
            List<string> toInclude = new List<string>();
            p.Eat("l curly");
            while (p.CurrentToken != "r curly")
            {
                toInclude.Add(p.GetIdentifier().ToLower());
                if (p.CurrentToken != "r curly")
                    p.Eat("comma");
            }
            p.Eat();
            toInclude.Reverse();

            Lexer.inputText = Lexer.inputText.Substring(Lexer.pointer);
            Lexer.pointer = 0;
            foreach (var file in toInclude)
            {
                if (p.imported.Contains(file)) return;

                Lexer.inputText = System.IO.File.ReadAllText("std/" + file + ".hpy") + "\n" + Lexer.inputText;
                p.imported.Add(file);
            }
        }

        public void RegisterGlobalVariable()
        {
            string name = p.Eat<string>("identifier");

            if (Variables.variables.Exists(x => x.name == name))
            {
                throw p.Exception("Global variable already exists: " + name);
            }

            p.Eat("colon");
            var type = Value.ParseType(p.GetIdentifier());

            if (Value.TypeEquals(type, Value.BatchType.Void))
            {
                throw p.Exception("Cannot declare a variable of type Void");
            }

            Variables.Create(name, type);
        }

        public static string ToTitleCase(string str)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        public Evaluator(Parser interpreter)
        {
            p = interpreter;
            interpreter.evaluator = this;
            
            var includeStatement = new Operation("include")
            {
                canBeGlobal = true,
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    List<string> toInclude = new List<string>();
                    p.Eat("l curly");
                    while (p.CurrentToken != "r curly")
                    {
                        toInclude.Add(p.GetIdentifier().ToLower());
                        if (p.CurrentToken != "r curly")
                            p.Eat("comma");
                    }
                    p.Eat();
                    /*toInclude.Reverse();

                    Lexer.inputText = Lexer.inputText.Substring(Lexer.pointer);
                    Lexer.pointer = 0;
                    foreach (var file in toInclude)
                    {
                        Lexer.inputText = System.IO.File.ReadAllText(file + ".hpy") + "\n" + Lexer.inputText;
                    }

                    p.lastLexerPoint = 0;
                    p.PreviousToken = null;
                    p.CurrentToken = Lexer.GetNextToken();*/

                    return Value.VOID;
                }
            };

            var functionDeclaration = new Operation("function")
            {
                canBeGlobal = true,
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    if(p.depth > 0)
                    {
                        throw p.Exception("Can only declare a function in the global scope");
                    }
                    
                    if(p.CurrentToken == "inline")
                    {
                        p.Eat();
                    }
                    
                    string name = p.GetIdentifier();
                    
                    List<string> argNames = new List<string>();
                    List<Value.BatchType> argTypes = new List<Value.BatchType>();

                    p.Eat("l bracket");
                    while (p.CurrentToken != "r bracket")
                    {
                        argNames.Add(p.Eat<string>("identifier"));
                        p.Eat("colon");
                        argTypes.Add(Value.ParseType(p.GetIdentifier()));

                        if (p.CurrentToken != "r bracket")
                            p.Eat("comma");
                    }
                    p.Eat();

                    Value.BatchType returnType = Value.BatchType.Void;
                    if (p.CurrentToken == "colon")
                    {
                        p.Eat("colon");
                        returnType = Value.ParseType(p.GetIdentifier());
                    }
                    
                    var func = UserFunctions.Get(name, argTypes);
                    int id = UserFunctions.GetID(func);

                    Variables.suffix = "_F" + id;

                    int index = p.output.Length;
                    string originalOutput = p.output;
                    
                    if (func.inline)
                    {
                        for (int i = 0; i < argNames.Count; i++)
                        {
                            Variables.Create(argNames[i] + "_I" + id, argTypes[i]);
                        }
                    }
                    else
                    {
                        p.Comment("BEGIN FUNCTION DECLARATION: " + UserFunctions.GetSignature(name, argTypes) + ": " + returnType.ToString().ToLower());
                        p.output += ":func_" + id + "\n";
                        for (int i = 0; i < argNames.Count; i++)
                        {
                            Variables.Create(argNames[i], new Value(argTypes[i], "%~" + (i + 1)), 1);
                        }
                    }

                    currentContext = func;

                    p.EvaluateBlock();
                    
                    currentContext = null;

                    if (!func.inline)
                    {
                        if (!p.output.Trim().EndsWith("goto :EOF"))
                        {
                            //Variables.variables.Exists(x => false);
                            Variables.DeleteOutOfScopeVariables();
                            p.output += "goto :EOF\n";
                        }
                    }

                    //p.Comment("END FUNCTION DECLARATION");

                    if(UserFunctions.IsRecursive(func, func))
                    {
                        throw p.Exception("Recursion is forbidden");
                    }

                    Variables.suffix = "";
                    
                    string newOutput = p.output;
                    p.output = originalOutput;
                    string code = newOutput.Substring(index);

                    func.body = code;

                    return Value.VOID;
                }
            };

            var variableDeclaration = new Operation("var")
            {
                canBeGlobal = true,
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    int depth = p.depth;

                    string name;
                    Value val;

                    if (depth == 0)
                    {
                        if (p.CurrentToken == "global") p.Eat();

                        name = p.Eat<string>("identifier");
                        var v = Variables.Get(name);
                        p.Eat("colon");
                        var type = Value.ParseType(p.GetIdentifier());
                        
                        if(p.CurrentToken == "equals")
                        {
                            p.Eat();
                            val = Evaluate();

                            if (type != val.type)
                            {
                                throw p.Exception("Attempt to assign value of type " + val.type + " to a variable of type " + type);
                            }

                            Variables.Assign(name, val);
                        }

                        return Value.VOID;
                    }

                    if(p.CurrentToken == "global")
                    {
                        p.Eat();
                        depth = 0;
                    }
                    
                    name = p.Eat<string>("identifier");

                    if(Variables.variables.Exists(x => x.name == name))
                    {
                        //throw p.Exception("Variable already exists in this scope: " + name);
                    }
                    
                    if (p.CurrentToken == "colon")
                    {
                        p.Eat();
                        var type = Value.ParseType(p.GetIdentifier());

                        if (Value.TypeEquals(type, Value.BatchType.Void))
                        {
                            throw p.Exception("Cannot declare a variable of type Void");
                        }

                        if (p.CurrentToken != "equals")
                        {
                            Variables.Create(name, type, depth);
                            return Value.VOID;
                        }

                        p.Eat("equals");
                        val = Evaluate();

                        if (type != val.type)
                        {
                            throw p.Exception("Attempt to assign value of type " + val.type + " to a variable of type " + type);
                        }

                        Variables.Create(name, val, depth);
                    }
                    else
                    {
                        if(depth == 0)
                        {
                            throw p.Exception("Variables in the global scope must have their types specified");
                        }

                        p.Eat("equals");
                        val = Evaluate();

                        if (Value.TypeEquals(val.type, Value.BatchType.Void))
                        {
                            throw p.Exception("Cannot declare a variable of type Void");
                        }

                        if (depth > 0) Variables.Create(name, val, depth);
                    }
                    
                    return Value.VOID;
                }
            };

            var numberLiteral = new Operation("number")
            {
                eatOperator = false,
                association = Operation.Association.None,
                unaryFunction = (none) => new Value(Value.BatchType.Int, p.Eat<float>().ToString())
            };

            var stringLiteral = new Operation("string")
            {
                eatOperator = false,
                association = Operation.Association.None,
                unaryFunction = (none) => new Value(Value.BatchType.String, p.Eat<string>())
            };
            
            var boolLiteral = new Operation("bool")
            {
                eatOperator = false,
                association = Operation.Association.None,
                unaryFunction = (none) => new Value(Value.BatchType.Bool, p.Eat<bool>() ? "1==1" : "1==0")
            };
            
            var bracket = new Operation("l bracket")
            {
                association = Operation.Association.None,
                unaryFunction = (none) => {
                    var outp = Evaluate();
                    p.Eat("r bracket");

                    if(outp.type == Value.BatchType.Int)
                    {
                        return new Value(Value.BatchType.Int, "(" + outp.value + ")");
                    }

                    var temp = Variables.CreateTemporary(outp.value);
                    return new Value(outp.type, temp);
                }
            };

            var neg = new Operation("minus")
            {
                association = Operation.Association.Right,
                unaryFunction = (right) => new Value(Value.BatchType.Int, "-" + right)
            };
            
            var mul = new Operation("multiply")
            {
                binaryFunction = (left, right) => binaryOp(left, right, Value.BatchType.Int, "*")
            };

            var div = new Operation("divide")
            {
                binaryFunction = (left, right) => binaryOp(left, right, Value.BatchType.Int, "/")
            };

            var mod = new Operation("modulus")
            {
                binaryFunction = (left, right) => binaryOp(left, right, Value.BatchType.Int, "%%")
            };

            var add = new Operation("plus")
            {
                binaryFunction = (left, right) => binaryOp(left, right, Value.BatchType.Int, "+")
            };

            var concat = new Operation("concat")
            {
                binaryFunction = (left, right) => new Value(Value.BatchType.String, left.value + right.value)
            };

            var sub = new Operation("minus")
            {
                binaryFunction = (left, right) => binaryOp(left, right, Value.BatchType.Int, "-")
            };

            
            
            var identifier = new Operation("identifier")
            {
                eatOperator = false,
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    string name = p.Eat<string>("identifier").ToString();

                    if (StandardLibrary.Exists(name))
                    {
                        List<Value> args = new List<Value>();
                        p.Eat("l bracket");
                        while (p.CurrentToken != "r bracket")
                        {
                            args.Add(Evaluate());
                            if (p.CurrentToken != "r bracket")
                                p.Eat("comma");
                        }
                        p.Eat();
                        return StandardLibrary.CallFunction(name, args.ToArray());
                    }
                    else
                    {
                        if(p.CurrentToken == "l bracket")
                        {
                            p.Comment("FUNCTION CALL TO " + name);

                            List<Value> args = new List<Value>();
                            p.Eat("l bracket");
                            while (p.CurrentToken != "r bracket")
                            {
                                args.Add(Evaluate());
                                if (p.CurrentToken != "r bracket")
                                    p.Eat("comma");
                            }
                            p.Eat();

                            var func = UserFunctions.Get(name, args.Select(x => x.type).ToList());
                            int id = UserFunctions.GetID(func);
                            
                            currentContext?.calls.Add(func);

                            if(func.inline)
                            {
                                for (int i = 0; i < func.argumentNames.Count; i++)
                                {
                                    Variables.Create(func.argumentNames[i] + "_I" + id, args[i]);
                                }

                                p.output += func.body;

                                for (int i = 0; i < func.argumentNames.Count; i++)
                                {
                                    Variables.Delete(func.argumentNames[i] + "_I" + id);
                                }
                            }
                            else
                            {
                                p.output += "call :func_" + id + " " +
                                string.Join(" ", args.Select(x => 
                                    (x.type == Value.BatchType.String && x.value.Contains(" ")) ? "\"" + x.value + "\"" : x.value)) + "\n";
                            }

                            if(func.returnType == Value.BatchType.Void)
                            {
                                p.Comment("END FUNCTION CALL");
                                return Value.VOID;
                            }
                            else
                            {
                                //string temp = Variables.CreateTemporary("%return_value%");
                                p.Comment("END FUNCTION CALL");
                                return new Value(func.returnType, "%return_value%");
                            }
                        }

                        if(p.CurrentToken == "increment")
                        {
                            p.Eat();
                            Variables.VerifyType(name, Value.BatchType.Int);
                            var va = Variables.Get(name);
                            p.output += "set /a var_" + name + (va.depth > 0 ? Variables.suffix : "") + "+=1\n";
                            return Value.VOID;
                        }
                        else if (p.CurrentToken == "decrement")
                        {
                            p.Eat();
                            Variables.VerifyType(name, Value.BatchType.Int);
                            var va = Variables.Get(name);
                            p.output += "set /a var_" + name + (va.depth > 0 ? Variables.suffix : "") + "-=1\n";
                            return Value.VOID;
                        }
                        else if (p.CurrentToken == "add")
                        {
                            p.Eat();
                            var v = Evaluate();
                            var va = Variables.Get(name);
                            p.output += "set /a var_" + name + (va.depth > 0 ? Variables.suffix : "") + "+=" + v.value + "\n";
                            return Value.VOID;
                        }
                        else if (p.CurrentToken == "subtract")
                        {
                            p.Eat();
                            var v = Evaluate();
                            var va = Variables.Get(name);
                            p.output += "set /a var_" + name + (va.depth > 0 ? Variables.suffix : "") + "-=" + v.value + "\n";
                            return Value.VOID;
                        }
                        else if (p.CurrentToken == "string add")
                        {
                            p.Eat();
                            var v = Evaluate();
                            var va = Variables.Get(name);
                            p.output += "set var_" + name + (va.depth > 0 ? Variables.suffix : "") + "=!var_" + name + Variables.suffix + "!" + v.value + "\n";
                            return Value.VOID;
                        }
                        else if (p.CurrentToken == "inc mul")
                        {
                            p.Eat();
                            var v = Evaluate();
                            var va = Variables.Get(name);
                            p.output += "set /a var_" + name + (va.depth > 0 ? Variables.suffix : "") + "*=" + v.value + "\n";
                            return Value.VOID;
                        }
                        else if (p.CurrentToken == "inc div")
                        {
                            p.Eat();
                            var v = Evaluate();
                            var va = Variables.Get(name);
                            p.output += "set /a var_" + name + (va.depth > 0 ? Variables.suffix : "") + "/=" + v.value + "\n";
                            return Value.VOID;
                        }
                        else if(p.CurrentToken == "equals")
                        {
                            p.Eat();
                            var v = Evaluate();

                            if (!Variables.Exists(name) && !name.Contains("%"))
                                throw p.Exception("No such variable exists: " + name);

                            Variables.Assign(name, v);
                            return Value.VOID;
                        }
                        else
                        {
                            if(name.Contains("%"))
                            {
                                //var temp1 = Variables.CreateTemporary("var_" + name);
                                //var temp2 = Variables.CreateTemporary("");
                                //var temp2name = Variables.NameFromTempReference(temp2);
                                //p.EmitLn("call set " + temp2name + "=%%" + temp1 + "%%");
                                return new Value(Value.BatchType.Indeterminate, "!var_" + name + "!");
                            }
                            else
                            {
                                //return Variables.GetReference(name);
                                if (currentContext != null && currentContext.inline)
                                {
                                    return Variables.GetReference(name + "_I" + UserFunctions.GetID(currentContext));
                                }
                                else
                                {
                                    return Variables.GetReference(name);
                                }
                            }
                            //return Variables.GetReference(name);
                        }
                    }
                }
            };

            var openBracket = new Operation("l curly")
            {
                eatOperator = false,
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    p.Eat();
                    return new Value(Value.BatchType.Void, "");
                }
            };

            var closeBracket = new Operation("r curly")
            {
                eatOperator = false,
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    p.Eat();
                    return new Value(Value.BatchType.Void, "");
                }
            };

            var ifStatement = new Operation("if")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    int selfid = ++ifid;

                    var condition = EvaluateCondition();

                    string output = "";

                    string temp = Variables.CreateTemporary(condition.value);

                    output += "if " + temp + " (\n";
                    output += "goto if_" + selfid + "\n";
                    output += ")";

                    string ifContents = p.PreEvaluateBlock();

                    List<string> elseIfContents = new List<string>();
                    int elseifs = 0;
                    while(p.CurrentToken == "elseif")
                    {
                        elseifs++;
                        p.Eat();
                        condition = EvaluateCondition();
                        temp = Variables.CreateTemporary(condition.value);

                        output += " else (\nif " + temp + " (\n";
                        output += "goto if_" + selfid + "_elseif_" + elseifs + "\n";
                        output += ")";

                        elseIfContents.Add(p.PreEvaluateBlock());
                    }

                    string elseContents = null;
                    if(p.CurrentToken == "else")
                    {
                        p.Eat();
                        output += " else (\n";
                        output += "goto if_" + selfid + "_else\n";
                        output += ")";

                        elseContents = p.PreEvaluateBlock();
                    }
                    
                    for (int i = 0; i < elseifs; i++)
                    {
                        output += "\n)";
                    }

                    output += "\ngoto if_" + selfid + "_end";

                    output += "\n:if_" + selfid + "\n";
                    output += ifContents;
                    output += "goto if_" + selfid + "_end\n";

                    for (int i = 1; i <= elseifs; i++)
                    {
                        output += ":if_" + selfid + "_elseif_" + i + "\n";
                        output += elseIfContents[i - 1];
                        output += "goto if_" + selfid + "_end\n";
                    }

                    if (elseContents != null)
                    {
                        output += ":if_" + selfid + "_else\n";
                        output += elseContents;
                    }

                    output += ":if_" + selfid + "_end\n";

                    p.output += output;

                    return new Value(Value.BatchType.Void, "");
                }
            };

            var whileLoop = new Operation("while")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    int loop = ++loopid;

                    p.output += ":loop_" + loop + "\n";

                    var condition = EvaluateCondition();
                    var temp = Variables.CreateTemporary(condition.value);

                    p.output += "if not " + temp + " goto loop_" + loop + "_end\n";

                    p.loopIDs.Push(loop);

                    p.EvaluateBlock();

                    p.loopIDs.Pop();

                    p.output += "goto loop_" + loop + "\n";
                    p.output += ":loop_" + loop + "_end";

                    return new Value(Value.BatchType.Void, "");
                }
            };

            var untilLoop = new Operation("until")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    int loop = ++loopid;

                    p.output += ":loop_" + loop + "\n";

                    var condition = EvaluateCondition();
                    var temp = Variables.CreateTemporary(condition.value);

                    p.output += "if " + temp + " goto loop_" + loop + "_end\n";

                    p.loopIDs.Push(loop);

                    p.EvaluateBlock();

                    p.loopIDs.Pop();

                    p.output += "goto loop_" + loop + "\n";
                    p.output += ":loop_" + loop + "_end";


                    return new Value(Value.BatchType.Void, "");
                }
            };

            void VerifyType(Value v, Value.BatchType type)
            {
                if(v.type != type)
                {
                    throw p.Exception("Expected value of type " + type + ", not " + v.type);
                }
            }

            var forLoop = new Operation("from")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    var start = Evaluate();
                    VerifyType(start, Value.BatchType.Int);

                    p.Eat("to");
                    var end = Evaluate();
                    VerifyType(end, Value.BatchType.Int);

                    bool upwards;
                    try
                    {
                        upwards = int.Parse(start.value) < int.Parse(end.value);
                    }
                    catch
                    {
                        upwards = true;
                    }

                    Value step;
                    if (p.CurrentToken == "by")
                    {
                        p.Eat();
                        step = Evaluate();
                        VerifyType(step, Value.BatchType.Int);
                    }
                    else
                    {
                        step = new Value(Value.BatchType.Int, upwards ? "1" : "-1");
                    }

                    bool usingVariable = p.CurrentToken == "with";

                    string name = null;
                    string reference = null;

                    if (usingVariable)
                    {
                        p.Eat();
                        string varname = p.Eat<string>();

                        reference = Variables.Create(varname, start, p.depth).value; // %var_x%
                        name = "var_" + varname; // var_x
                    }
                    else
                    {
                        reference = Variables.CreateTemporaryInt(start.value); // %temp_x%
                        name = Variables.NameFromTempReference(reference); // temp_x
                    }

                    int loop = ++loopid;
                    p.output += ":loop" + loop + "\n";

                    p.output += "if not " + reference + " " + (upwards ? "LSS" : "GTR") + " " + end.value + " goto loop" + loop + "_end\n";
                    
                    p.EvaluateBlock();

                    p.loopIDs.Pop();
                    
                    p.output += "set /a " + name + (usingVariable ? Variables.suffix : "") + "+=" + step.value + "\n";
                    p.output += "goto loop" + loop + "\n";
                    p.output += ":loop" + loop + "_end\n";

                    if (usingVariable)
                        Variables.Delete(Variables.NameFromReference(reference));

                    return Value.VOID;
                }
            };

            var continueStatement = new Operation("continue")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    return new Value(Value.BatchType.Void, "goto loop" + p.loopIDs.Peek());
                }
            };

            var breakStatement = new Operation("break")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    return new Value(Value.BatchType.Void, "goto loop" + p.loopIDs.Peek() + "_end");
                }
            };

            var not = new Operation("not")
            {
                association = Operation.Association.Right,
                unaryFunction = (right) => new Value(Value.BatchType.Bool, "not " + right)
            };

            Value BoolOp(Value left, Value right, string op)
            {
                var tempL = Variables.FlexibleTemporary(left);
                var tempR = Variables.FlexibleTemporary(right);
                var tempResult = Variables.CreateTemporary("1==0");
                var name = Variables.NameFromTempReference(tempResult);
                p.EmitLn("if " + tempL + " " + op + " " + tempR + " set " + name + "=1==1");
                return new Value(Value.BatchType.Bool, tempResult);
            }

            Value IntComparisonOp(Value left, Value right, string op)
            {
                VerifyType(left, Value.BatchType.Int);
                VerifyType(right, Value.BatchType.Int);
                return BoolOp(left, right, op);
            }

            var eq = new Operation("double equal")
            {
                binaryFunction = (left, right) => BoolOp(left, right, "EQU")
            };
            
            var neq = new Operation("not equal")
            {
                binaryFunction = (left, right) => BoolOp(left, right, "NEQ")
            };

            var gr = new Operation("greater than")
            {
                binaryFunction = (left, right) => IntComparisonOp(left, right, "GTR")
            };

            var ls = new Operation("less than")
            {
                binaryFunction = (left, right) => IntComparisonOp(left, right, "LSS")
            };

            var geq = new Operation("greater or equal")
            {
                binaryFunction = (left, right) => IntComparisonOp(left, right, "GEQ")
            };

            var leq = new Operation("less or equal")
            {
                binaryFunction = (left, right) => IntComparisonOp(left, right, "LEQ")
            };

            var semicolon = new Operation("semicolon")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    return Value.VOID;
                }
            };

            var and = new Operation("and")
            {
                binaryFunction = (left, right) =>
                {
                    string reference = Variables.CreateTemporary("1==0");
                    string name = Variables.NameFromTempReference(reference);
                    //p.output += NewTempBoolVar("1==0");
                    //int id = varid;
                    p.output += "if " + left.value + " ( if " + right.value + " ( \n";
                    p.output += "set " + name + "=1==1\n";
                    p.output += ") \n)\n";

                    return new Value(Value.BatchType.Bool, reference);
                }
            };

            var or = new Operation("or")
            {
                binaryFunction = (left, right) =>
                {
                    string reference = Variables.CreateTemporary("1==0");
                    string name = Variables.NameFromTempReference(reference);
                    //p.output += NewTempBoolVar("1==1");
                    //int id = varid;
                    p.output += "if " + left.value + " set " + name + "=1==1\n";
                    p.output += "if " + right.value + " set " + name + "=1==1\n";
                    return new Value(Value.BatchType.Bool, reference);
                }
            };

            /*
            var arrayLiteral = new Operation("l square")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    string array = "";
                    while(p.CurrentToken != "r square")
                    {
                        array += Evaluate() + " ";

                        if (p.CurrentToken != "r square")
                        {
                            p.Eat("comma");
                        }
                    }
                    p.Eat();

                    return new Value(Value.BatchType.Array, array.TrimEnd());
                }
            };*/

            var returnStatement = new Operation("return")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    if(currentContext == null)
                    {
                        throw p.Exception("Can only return a value from within a function");
                    }

                    Value v;

                    if(p.CurrentToken == "semicolon")
                    {
                        v = Value.VOID;
                    }
                    else
                    {
                        v = Evaluate();
                        p.output += "set " + (v.type == Value.BatchType.Int ? "/a " : "") + "\"return_value=" + v.value + "\"\n";
                    }
                    
                    if(currentContext.returnType != v.type)
                    {
                        throw p.Exception("Must return value of type " + currentContext.returnType + ", not " + v.type);
                    }

                    //returnValueType = v.type;
                    int depth = p.depth;
                    p.depth = 0;
                    Variables.DeleteOutOfScopeVariables();
                    p.depth = depth;

                    /*foreach (var name in variablesDeclaredInsideFunction)
                    {
                        name.de
                    }*/

                    return new Value(Value.BatchType.Void, currentContext.inline ? "" : "goto :EOF");
                }
            };

            var referenceOf = new Operation("ampersand")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    var name = p.GetIdentifier();
                    if(name.Contains("%"))
                    {
                        return new Value(Value.BatchType.String, name);
                    }

                    var val = Variables.Get(name);
                    return new Value(Value.BatchType.String, name + (val.depth > 0 ? Variables.suffix : ""));
                }
            };

            var deleteStatement = new Operation("delete")
            {
                association = Operation.Association.None,
                unaryFunction = (none) =>
                {
                    var var = p.GetIdentifier();
                    Variables.Delete(var, true);
                    return Value.VOID;
                }
            };

            var asStatement = new Operation("as")
            {
                association = Operation.Association.Left,
                unaryFunction = (left) =>
                {
                    VerifyType(left, Value.BatchType.Indeterminate);
                    return new Value(Value.ParseType(p.GetIdentifier()), left.value);
                }
            };

            void register(Operation op)
            {
                precedences.Last().Add(op);
            }

            void precedence()
            {
                precedences.Add(new List<Operation>());
            }

            precedence();
            register(semicolon);

            precedence();
            register(referenceOf);
            //register(toStringLiteral);
            //register(thisLiteral);
            //register(baseLiteral);
            register(bracket);
            //register(undefinedLiteral);
            //register(arrayLiteral);
            register(numberLiteral);
            register(stringLiteral);
            register(boolLiteral);
            //register(tableLiteral);
            //register(charLiteral);
            ////register(instantiation);

            precedence();
            register(identifier);
            //register(methodCall);
            //register(traverse);
            //register(index);

            precedence();
            register(asStatement);

            precedence();
            register(variableDeclaration);

            precedence();
            register(neg);
            register(not);

            //precedence();
            //register(pow);

            precedence();
            register(mul);
            register(div);
            register(mod);

            precedence();
            register(concat);
            register(add);
            register(sub);

            precedence();
            register(eq);
            register(neq);
            register(gr);
            register(ls);
            register(geq);
            register(leq);

            precedence();
            register(and);

            precedence();
            register(or);

            //precedence();
            //register(conditional);

            precedence();
            register(closeBracket);
            register(openBracket);
            register(ifStatement);
            register(whileLoop);
            register(untilLoop);
            //register(includeStatement);
            //register(forEachLoop);
            register(continueStatement);
            register(breakStatement);
            register(functionDeclaration);
            //register(funcLiteral);
            register(returnStatement);
            register(deleteStatement);
            register(forLoop);
            //register(structureLiteral);
            //register(include);
            //register(tryCatch);
            //register(throwStatement);
        }

        public Value EvaluateCondition()
        {
            var e = Evaluate();

            if(e.type != Value.BatchType.Bool)
            {
                throw p.Exception("Condition must be a Boolean, not a " + e.type);
            }

            /*if(e.value == "true")
            {
                e.value = "1==1";
            }
            else if(e.value == "false")
            {
                e.value = "1==0";
            }*/

            return e;
        }

        public Value Evaluate()
        {
            var result = Evaluate(precedences.Count - 1);

            if (result == null)
            {
                throw new ParserException(guiltyToken, "Unexpected token: " + guiltyToken);
            }

            return result;
        }
        
        private Token guiltyToken;

        private bool OperatorEqualsCurrentToken(Operation x)
        {
            return x.operatorToken == p.CurrentToken.Type;
        }

        public Value Evaluate(int precedence)
        {
            guiltyToken = null;
            if (precedence < 0)
            {
                guiltyToken = p.CurrentToken;
                return null;
            }

            Value result = Evaluate(precedence - 1);

            List<Operation> operations = new List<Operation>(precedences[precedence]);

            void InvalidateOperation(bool ateOperator, Operation o)
            {
                operations.Remove(o);
                if (ateOperator)
                {
                    Lexer.pointer = p.lastLexerPoint;
                    p.CurrentToken = p.PreviousToken;
                }
            }
            
            while(operations.Exists(OperatorEqualsCurrentToken))
            {
                var operation = operations.Find(OperatorEqualsCurrentToken);

                if (operation.eatOperator)
                {
                    p.Eat();
                }
                
                if (operation.IsBinary)
                {
                    result = operation.binaryFunction.Invoke(result, Evaluate(precedence - 1));
                }
                else if (operation.association == Operation.Association.Right)
                {
                    if (result != null)
                    {
                        InvalidateOperation(operation.eatOperator, operation);
                        continue;
                    }
                    
                    result = operation.unaryFunction.Invoke(Evaluate(precedence - 1));
                }
                else if (operation.association == Operation.Association.Left)
                {
                    if (result == null)
                    {
                        InvalidateOperation(operation.eatOperator, operation);
                        continue;
                    }
                    
                    result = operation.unaryFunction.Invoke(result);
                }
                else if (operation.association == Operation.Association.None)
                {
                    if (result != null)
                    {
                        InvalidateOperation(operation.eatOperator, operation);
                        continue;
                    }
                    
                    result = operation.unaryFunction.Invoke(null);
                }
                else
                {
                    throw Parser.current.Exception("Operator is unary but has an unassigned association");
                }

                operations = new List<Operation>(precedences[precedence]);
            }

            if (result == null)
            {
                return Evaluate(precedence - 1);
            }
            
            return result;
        }

    }
}
