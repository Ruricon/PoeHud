namespace PoeHUD.Poe.UI
{
    public class Label : Element
    {
        public string Text
        {
            get
            {
                int num = M.ReadInt(Address + 2468);
                if (num <= 0 || num > 256)
                {
                    return "";
                }
                if (num >= 8)
                {
                    return M.ReadStringU(M.ReadInt(Address + 2452), num*2);
                }
                return M.ReadStringU(Address + 2452, num*2);
            }
        }
    }
}