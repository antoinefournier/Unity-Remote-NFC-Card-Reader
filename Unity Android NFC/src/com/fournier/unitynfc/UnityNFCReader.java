package com.fournier.unitynfc;

import java.util.Arrays;

import android.app.PendingIntent;
import android.content.Intent;
import android.content.IntentFilter;
import android.nfc.NdefMessage;
import android.nfc.NdefRecord;
import android.nfc.NfcAdapter;
import android.nfc.tech.NfcF;
import android.os.Build;
import android.os.Bundle;
import android.os.Parcelable;
import android.util.Log;
import android.view.View;
import android.view.WindowManager;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerNativeActivity;

/**
 * Get the data from scanned NFC Tag and send them to Unity.
 * @author Antoine Fournier
 */
public class UnityNFCReader extends UnityPlayerNativeActivity
{
	private NfcAdapter mNfcAdapter;
    private PendingIntent mPendingIntent;
    private IntentFilter[] mIntentFilter;
    private String[][] mTechListsArray;
    

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);

        // Then PendingIntent will receive the scanned tag's data
        mPendingIntent = PendingIntent.getActivity(UnityNFCReader.this, 0, new Intent(UnityNFCReader.this,
        		getClass()).addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP), 0);
        
        // Type of intents we want to intercept
        mIntentFilter = new IntentFilter[] { new IntentFilter(NfcAdapter.ACTION_TAG_DISCOVERED) };
        
        // List of technology we can handle
        mTechListsArray = new String[][] { new String[] { NfcF.class.getName() } };
        
        // Check the state of the NFNFC ship
        mNfcAdapter = NfcAdapter.getDefaultAdapter(this);
        if (mNfcAdapter == null)
        {
            Log.e(UnityNFCReader.class.toString(), "This device doesn't support NFC.");
            finish();
            return;
        }
        if (!mNfcAdapter.isEnabled())
        {
            Log.e(UnityNFCReader.class.toString(), "NFC is disabled.");
        }
        else
        {
            Log.i(UnityNFCReader.class.toString(), "NFC reader initialized.");
        }
    }

    @Override
    public void onResume()
    {
        super.onResume();
        mNfcAdapter.enableForegroundDispatch(this, mPendingIntent, mIntentFilter, mTechListsArray);
    }
    
    @Override
    public void onPause()
    {
        super.onPause();
        mNfcAdapter.disableForegroundDispatch(this);
    }

    @Override
	public void onWindowFocusChanged(boolean hasFocus)
    {
	    super.onWindowFocusChanged(hasFocus);
	    
	    // Set fullscreen
	    if (hasFocus && Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT)
	    {
	        getWindow().getDecorView().setSystemUiVisibility(
	                  View.SYSTEM_UI_FLAG_LAYOUT_STABLE
	                | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
	                | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
	                | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
	                | View.SYSTEM_UI_FLAG_FULLSCREEN
	                | View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY);
	    }
	}
    
    @Override
    protected void onNewIntent(Intent intent)
    {
        // A tag has been scanned
        if (intent.getAction().equals(NfcAdapter.ACTION_TAG_DISCOVERED))
        {
            Log.v("Unity NFC Reader", "NFC tag discovered.");

        	//String id = ByteArrayToHexString(intent.getByteArrayExtra(NfcAdapter.EXTRA_ID));
            Parcelable[] messageData = intent.getParcelableArrayExtra(NfcAdapter.EXTRA_NDEF_MESSAGES);
            String message = "";

            try
            {
            	if (messageData == null)
            	{
                    Log.e("Unity NFC Reader", "Nothing message found on the tag");
            	}
            	else
            	{
		            for (int i = 0; i < messageData.length; i++)
		            {
		                NdefRecord[] record = ((NdefMessage)messageData[i]).getRecords();
		                
		                for (int j = 0; j < record.length; j++)
		                {
		                    if (record[j].getTnf() == NdefRecord.TNF_WELL_KNOWN && Arrays.equals(record[j].getType(), NdefRecord.RTD_TEXT))
		                    {
		                        byte[] payload = record[j].getPayload();
		                        String textEncoding = ((payload[0] & 0200) == 0) ? "UTF-8" : "UTF-16";
		                        int langCodeLen = payload[0] & 0077;
		                        
		                        message += new String(payload, langCodeLen + 1, payload.length - langCodeLen - 1, textEncoding);
		                    }
		                }
		            }
            	}
            }
            catch (Exception e)
            {
                Log.e("Unity NFC Reader", "Error while reading : " + e.toString());
                message += e.toString();
            }
        	
        	// Send the data to Unity
            // Arg1 : Unity GameObject name that will receive the event
            // Arg2 : Method called on the GameObject
            // Arg3 : Data to send
        	UnityPlayer.UnitySendMessage("Client", "OnNfcTagScanned", message);
		}
    }
    /*
    private String ByteArrayToHexString(byte[] inarray)
    {
	    int i, j, in;
	    String [] hex = {"0","1","2","3","4","5","6","7","8","9","A","B","C","D","E","F"};
	    String out= "";
	
	    for(j = 0 ; j < inarray.length ; ++j) 
	    {
	        in = (int) inarray[j] & 0xff;
	        i = (in >> 4) & 0x0f;
	        out += hex[i];
	        i = in & 0x0f;
	        out += hex[i];
	    }
	    return out;
	}*/
}
