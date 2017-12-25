using System;
using System.Diagnostics;

namespace BattleBot
{
    public static class Dice
    {
        private enum Operator { Add, Subtract, Multiply, Divide }


        public static string Flip(string author)
        {
            Random rdm = new Random();

            if (rdm.Next(2) == 0)
            {
                return $"{author}'s coin landed **heads** up.";
            }
            return $"{author}'s coin landed **tail** up.";
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
                            root = newExpression;
                        }
                        else if (curExpression is NumExpression)
                        {
                            ((NumExpression) curExpression).Right = newExpression;
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
                            ((NumExpression) curExpression).Right = rndExpression;
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
                            intExp.Left = curExpression;
                            curExpression = intExp;
                            root = intExp;
                        }
                        else if (curExpression != null && intExp != null)
                        {
                            if (((NumExpression)curExpression).opr == Operator.Add || ((NumExpression)curExpression).opr == Operator.Subtract)
                            {
                                if (intExp.opr == Operator.Add || intExp.opr == Operator.Subtract)
                                {
                                    intExp.Left = root;
                                    root = intExp;
                                    curExpression = intExp;
                                }
                                else
                                {
                                    intExp.Left = (curExpression as NumExpression).Right;
                                    ((NumExpression)curExpression).Right = intExp;
                                    curExpression = intExp;
                                }
                            }
                            else
                            {
                                intExp.Left = root;
                                root = intExp;
                                curExpression = intExp;
                            }
                        }
                        break;
                    default:
                        return "Could not parse that expression. " + Emotes.Confused;
                }
            }

            if (root != null)
            {
                double sum = root.Calculate();

                if (root is NumExpression)
                {
                    return $"{author} rolled **{root.GetResult()}**, total: **{sum}**.";
                }

                return $"{author} rolled **{root.GetResult()}**.";
            }
            return "Not a valid expression.";
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
                        index = i - 1;
                        return int.Parse(number);
                }
            }
            index = i - 1;
            return int.Parse(number);
        }

        private abstract class Expression
        {
            public abstract string GetExpression();
            public abstract string GetResult();
            public abstract double Calculate();
        }

        private class IntVal : Expression
        {
            private int Value;

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

        private class RdmVal : Expression
        {
            private int _result;
            private readonly int _diceSize;

            public RdmVal(int diceSize)
            {
                this._diceSize = diceSize;
            }

            public override double Calculate()
            {
                if (_result == 0)
                {
                    Random rng = new Random();
                    _result = rng.Next(1, _diceSize+1);
                    return _result;
                }
                return _result;
            }

            public override string GetExpression()
            {
                return "d" + _diceSize;
            }

            public override string GetResult()
            {
                return "" + _result;
            }
        }

        private class NumExpression : Expression
        {
            private double _result;
            public Operator opr;

            public Expression Left;
            public Expression Right;

            public NumExpression(Operator opr)
            {
                this.opr = opr;
            }

            public override double Calculate()
            {
                if (_result <= 0)
                {
                    switch (opr)
                    {
                        case Operator.Add:
                            _result = Left.Calculate() + Right.Calculate();
                            break;
                        case Operator.Subtract:
                            _result = Left.Calculate() - Right.Calculate();
                            break;
                        case Operator.Multiply:
                            _result = Left.Calculate() * Right.Calculate();
                            break;
                        case Operator.Divide:
                            _result = Left.Calculate() / Right.Calculate();
                            break;
                    }
                    return _result;

                }
                return _result;
            }

            public override string GetExpression()
            {
                switch (opr)
                {
                    case Operator.Add:
                        return Left.GetExpression() + " + " + Right.GetExpression();
                    case Operator.Subtract:
                        return Left.GetExpression() + " - " + Right.GetExpression();
                    case Operator.Multiply:
                        return Left.GetExpression() + " * " + Right.GetExpression();
                    case Operator.Divide:
                        return Left.GetExpression() + " / " + Right.GetExpression();
                    default:
                        return "";
                }
            }

            public override string GetResult()
            {
                switch (opr)
                {
                    case Operator.Add:
                        return Left.GetResult() + " + " + Right.GetResult();
                    case Operator.Subtract:
                        return Left.GetResult() + " - " + Right.GetResult();
                    case Operator.Multiply:
                        return Left.GetResult() + " * " + Right.GetResult();
                    case Operator.Divide:
                        return Left.GetResult() + " / " + Right.GetResult();
                    default:
                        return "";
                }
            }
        }
    }
}
