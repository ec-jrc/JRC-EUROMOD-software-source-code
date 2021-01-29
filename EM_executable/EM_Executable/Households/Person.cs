namespace EM_Executable
{
    internal class Person
    {
        internal Person(byte _indexInHH) { indexInHH = _indexInHH; }

        internal double tuHeadID = double.MaxValue;
        internal byte indexInHH = byte.MaxValue;
        internal byte partnerIndexInHH = byte.MaxValue;
        internal bool isHead = false;
        internal bool isPartner = false;
        internal bool isDepChild = false;
        internal bool isOwnChild = false;
        internal bool isOwnDepChild = false;
        internal bool isDepParent = false;
        internal bool isDepRelative = false;
        internal bool isLoneParent = false;
        internal bool isLooseDepChild = false;
        internal bool isDepMember = false; // This is used for flagging people that were added in the TU as "depOfdep"
        internal HeadCalculation headCalculation = new HeadCalculation(); 

        // this inner class is used by FunDefTU and specifically by the GetHead() function
        internal class HeadCalculation 
        {
            internal bool calculated = false;
            internal double income;
            internal double age;
        }
    }
}
