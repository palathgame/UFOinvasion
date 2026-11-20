
public class DMGBuff : BuffScript
{

    protected override string Label()
    {
        return positiv ? "DMG +5" : "DMG -3";
    }

    public int getStatDMG()
    {

        if (positiv)
        {

            return 5;
        }

        return -3;

    }



}
