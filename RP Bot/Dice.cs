using System;
using System.Collections.Generic;
using System.Text;

namespace BattleBot
{
    public static class Dice
    {
        enum Operator { Add, Subtract, Multiply, Divide };


        public static string Flip(string author)
        {
            Random rdm = new Random();

            if (rdm.Next(2) == 0)
            {
                return $"{author}'s coin landed **heads** up.";
            } else
            {
                return $"{author}'s coin landed **tail** up.";
            }

        }

        public static string Roll(string author, string cmd)
        {
            Expression root = null;
            Expression curExpression = null;

            for (int i = 0; i < cmd.Length; i++)
            {

                switch (cmd[i])
                {
                    case ' ':
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        int intValue = ReadInt(cmd, i, out i);
                        IntVal newExpression = new IntVal(intValue);
                        if (curExpression == null)
                        {
                            curExpression = newExpression;
                            if (root == null)
                            {
                                root = newExpression;
                            }
                        }
                        else if (curExpression is NumExpression)
                        {
                            (curExpression as NumExpression).right = newExpression;
                        }
                        break;
                    case 'd':
                        int rndValue = ReadInt(cmd, i + 1, out i);
                        if (rndValue == 0)
                        {
                            return "Cannot roll a 0-sided dice.";
                        }
                        RdmVal rndExpression = new RdmVal(rndValue);
                        if (curExpression == null)
                        {
                            curExpression = rndExpression;
                            root = rndExpression;
                        }
                        else if (curExpression is NumExpression)
                        {
                            (curExpression as NumExpression).right = rndExpression;
                        }
                        break;
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                        NumExpression intExp = null;
                        switch (cmd[i])
                        {
                            case '+':
                                intExp = new NumExpression(Operator.Add);
                                break;
                            case '-':
                                intExp = new NumExpression(Operator.Subtract);
                                break;
                            case '*':
                                intExp = new NumExpression(Operator.Multiply);
                                break;
                            case '/':
                                intExp = new NumExpression(Operator.Divide);
                                break;
                        }

                        if (curExpression is RdmVal || curExpression is IntVal)
                        {
                            intExp.left = curExpression;
                            curExpression = intExp;
                            root = intExp;
                        }
                        else
                        {
                            if ((curExpression as NumExpression).opr == Operator.Add || (curExpression as NumExpression).opr == Operator.Subtract)
                            {
                                if (intExp.opr == Operator.Add || intExp.opr == Operator.Subtract)
                                {
                                    intExp.left = root;
                                    root = intExp;
                                    curExpression = intExp;
                                }
                                else
                                {
                                    intExp.left = (curExpression as NumExpression).right;
                                    (curExpression as NumExpression).right = intExp;
                                    curExpression = intExp;
                                }
                            }
                            else
                            {
                                intExp.left = root;
                                root = intExp;
                                curExpression = intExp;
                            }
                        }
                        break;
                    default:
                        return "Could not parse that expression. " + Emotes.Confused;
                }
            }

            double sum = root.Calculate();

            return $"{author} rolled **{root.GetResult()}**, total: **{sum}**.";
        }

        private static int ReadInt(string cmd, int i, out int index)
        {
            string number = "";
            for (; i < cmd.Length; i++)
            {
                switch (cmd[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        number += cmd[i];
                        break;
                    default:
                        index = i-1;
                        return int.Parse(number);
                }
            }
            index = i-1;
            return int.Parse(number);
        }

        abstract class Expression
        {
            public abstract string GetExpression();
            public abstract string GetResult();
            public abstract double Calculate();
        }

        class IntVal : Expression
        {
            int Value;

            public IntVal(int value)
            {
                Value = value;
            }

            public override double Calculate()
            {
                return Value;
            }

            public override string GetExpression()
            {
                return "" + Value;
            }

            public override string GetResult()
            {
                return "" + Value;
            }
        }

        class RdmVal : Expression
        {
            int result = 0;
            int diceSize;

            public RdmVal(int diceSize)
            {
                this.diceSize = diceSize;
            }

            public override double Calculate()
            {
                if (result == 0)
                {
                    Random rng = new Random();
                    result = rng.Next(1, diceSize);
                    return result;
                }
                else
                {
                    return result;
                }
            }

            public override string GetExpression()
            {
                return "d" + diceSize;
            }

            public override string GetResult()
            {
                return "" + result;
            }
        }

        class NumExpression : Expression
        {
            double result = 0;
            public Operator opr;

            public Expression left;
            public Expression right;

            public NumExpression(Operator opr)
            {
                this.opr = opr;
            }

            public override double Calculate()
            {
                if (result == 0)
                {
                    switch (opr)
                    {
                        case Operator.Add:
                            result = left.Calculate() + right.Calculate();
                            break;
                        case Operator.Subtract:
                            result = left.Calculate() - right.Calculate();
                            break;
                        case Operator.Multiply:
                            result = left.Calculate() * right.Calculate();
                            break;
                        case Operator.Divide:
                            result = left.Calculate() / (double) right.Calculate();
                            break;
                        default:
                            break;
                    }
                    return result;

                }
                else
                {
                    return result;
                }
            }

            public override string GetExpression()
            {
                switch (opr)
                {
                    case Operator.Add:
                        return left.GetExpression() + " + " + right.GetExpression();
                    case Operator.Subtract:
                        return left.GetExpression() + " - " + right.GetExpression();
                    case Operator.Multiply:
                        return left.GetExpression() + " * " + right.GetExpression();
                    case Operator.Divide:
                        return left.GetExpression() + " / " + right.GetExpression();
                    default:
                        return "";
                }
            }

            public override string GetResult()
            {
                switch (opr)
                {
                    case Operator.Add:
                        return left.GetResult() + " + " + right.GetResult();
                    case Operator.Subtract:
                        return left.GetResult() + " - " + right.GetResult();
                    case Operator.Multiply:
                        return left.GetResult() + " * " + right.GetResult();
                    case Operator.Divide:
                        return left.GetResult() + " / " + right.GetResult();
                    default:
                        return "";
                }
            }
        }
    }
}
