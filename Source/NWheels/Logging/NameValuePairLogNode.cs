﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Logging
{
    public class NameValuePairLogNode : LogNode
    {
        private readonly Exception _exception;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(string messageId, LogLevel level, Exception exception)
            : base(messageId, LogContentTypes.Text, level)
        {
            _exception = exception;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            if ( _exception != null )
            {
                return _exception.ToString();
            }
            else
            {
                return null;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    internal static class LogNameValuePairHelper
    {
        public static string AppendToSingleLineText<T>(this string s, ref LogNameValuePair<T> pair, ref bool anyAppended)
        {
            if ( !pair.IsDetail )
            {
                var result = s + (anyAppended ? ", " : ": ") + pair.FormatLogString();
                anyAppended = true;
                return result;
            }
            else
            {
                return s;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string AppendToFullDetailsText<T>(this string s, ref LogNameValuePair<T> pair)
        {
            if ( pair.IsDetail )
            {
                return (s != "" ? System.Environment.NewLine : "") + pair.FormatLogString() + System.Environment.NewLine;
            }
            else
            {
                return s;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string AppendToExceptionMessage<T>(this string s, ref LogNameValuePair<T> pair, ref bool anyAppended)
        {
            var result = s + (anyAppended ? ", " : ": ") + pair.FormatLogString();
            anyAppended = true;
            return result;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public struct LogNameValuePair<T>
    {
        public string Name;
        public T Value;
        public bool IsDetail;
        public string Format;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Pure]
        public string FormatValue()
        {
            var formattable = Value as IFormattable;

            if ( Format != null && formattable != null )
            {
                return formattable.ToString(Format, CultureInfo.CurrentCulture);
            }
            else if ( !typeof(T).IsValueType && (object)Value == null )
            {
                return "null";
            }
            else
            {
                return Value.ToString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FormatLogString()
        {
            return LogNode.FormatNameValuePair(this.Name, this.FormatValue());
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(string messageId, LogLevel level, Exception exception, LogNameValuePair<T1> value1)
            : base(messageId, level, exception)
        {
            _value1 = value1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return string.Empty
                .AppendToFullDetailsText(ref _value1) +
                base.FormatFullDetailsText()
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(string messageId, LogLevel level, Exception exception, LogNameValuePair<T1> value1, LogNameValuePair<T2> value2)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2) +
                base.FormatFullDetailsText()
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return 
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3) +
                base.FormatFullDetailsText()
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended)
                .AppendToSingleLineText(ref _value4, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3)
                .AppendToFullDetailsText(ref _value4) +
                base.FormatFullDetailsText()
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString() + delimiter +
                _value4.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended)
                .AppendToSingleLineText(ref _value4, ref anyAppended)
                .AppendToSingleLineText(ref _value5, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3)
                .AppendToFullDetailsText(ref _value4)
                .AppendToFullDetailsText(ref _value5) +
                base.FormatFullDetailsText()
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString() + delimiter +
                _value4.FormatLogString() + delimiter +
                _value5.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5, T6> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;
        private LogNameValuePair<T6> _value6;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
            _value6 = value6;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended)
                .AppendToSingleLineText(ref _value4, ref anyAppended)
                .AppendToSingleLineText(ref _value5, ref anyAppended)
                .AppendToSingleLineText(ref _value6, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3)
                .AppendToFullDetailsText(ref _value4)
                .AppendToFullDetailsText(ref _value5)
                .AppendToFullDetailsText(ref _value6) +
                base.FormatFullDetailsText()
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString() + delimiter +
                _value4.FormatLogString() + delimiter +
                _value5.FormatLogString() + delimiter +
                _value6.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5, T6, T7> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;
        private LogNameValuePair<T6> _value6;
        private LogNameValuePair<T7> _value7;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6, LogNameValuePair<T7> value7)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
            _value6 = value6;
            _value7 = value7;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended)
                .AppendToSingleLineText(ref _value4, ref anyAppended)
                .AppendToSingleLineText(ref _value5, ref anyAppended)
                .AppendToSingleLineText(ref _value6, ref anyAppended)
                .AppendToSingleLineText(ref _value7, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3)
                .AppendToFullDetailsText(ref _value4)
                .AppendToFullDetailsText(ref _value5)
                .AppendToFullDetailsText(ref _value6)
                .AppendToFullDetailsText(ref _value7) +
                base.FormatFullDetailsText()
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString() + delimiter +
                _value4.FormatLogString() + delimiter +
                _value5.FormatLogString() + delimiter +
                _value6.FormatLogString() + delimiter +
                _value7.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5, T6, T7, T8> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;
        private LogNameValuePair<T6> _value6;
        private LogNameValuePair<T7> _value7;
        private LogNameValuePair<T8> _value8;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6, LogNameValuePair<T7> value7, LogNameValuePair<T8> value8)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
            _value6 = value6;
            _value7 = value7;
            _value8 = value8;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended)
                .AppendToSingleLineText(ref _value4, ref anyAppended)
                .AppendToSingleLineText(ref _value5, ref anyAppended)
                .AppendToSingleLineText(ref _value6, ref anyAppended)
                .AppendToSingleLineText(ref _value7, ref anyAppended)
                .AppendToSingleLineText(ref _value8, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3)
                .AppendToFullDetailsText(ref _value4)
                .AppendToFullDetailsText(ref _value5)
                .AppendToFullDetailsText(ref _value6)
                .AppendToFullDetailsText(ref _value7)
                .AppendToFullDetailsText(ref _value8) +
                base.FormatFullDetailsText()
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString() + delimiter +
                _value4.FormatLogString() + delimiter +
                _value5.FormatLogString() + delimiter +
                _value6.FormatLogString() + delimiter +
                _value7.FormatLogString() + delimiter +
                _value8.FormatLogString();
        }
    }
}