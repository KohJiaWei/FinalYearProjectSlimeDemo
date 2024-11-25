using LSL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadingMuse2 : MonoBehaviour

{
    
    // We need to find the stream somehow. You must provide a StreamName in editor or before this object is Started.
    public string StreamName;
    ContinuousResolver resolver;

    double max_chunk_duration = 0.2;  // Duration, in seconds, of buffer passed to pull_chunk. This must be > than average frame interval.

    // We need to keep track of the inlet once it is resolved.
    private StreamInlet inlet;

    // We need buffers to pass to LSL when pulling data.
    private float[,] data_buffer;  // Note it's a 2D Array, not array of arrays. Each element has to be indexed specifically, no frames/columns.
    private double[] timestamp_buffer;


    IEnumerator ResolveExpectedStream()
    {
        //Debug.Log("does it reach here?");
        var results = resolver.results();
        while (results.Length == 0)
        {
            //Debug.Log("while loop");
            yield return new WaitForSeconds(.1f);
            results = resolver.results();
        }

        inlet = new StreamInlet(results[0]);

        // Prepare pull_chunk buffer
        int buf_samples = (int)Mathf.Ceil((float)(inlet.info().nominal_srate() * max_chunk_duration));
        // Debug.Log("Allocating buffers to receive " + buf_samples + " samples.");
        int n_channels = inlet.info().channel_count();
        data_buffer = new float[buf_samples, n_channels];
        timestamp_buffer = new double[buf_samples];
    }

    void Start()
    {
        if (!StreamName.Equals(""))
            resolver = new ContinuousResolver("name", StreamName);
        else
        {
            //Debug.LogError("Object must specify a name for resolver to lookup a stream.");
            this.enabled = false;
            return;
        }
        StartCoroutine(ResolveExpectedStream());
    }

    void Update()
    {
        //Debug.Log(inlet);
        if (inlet != null)
        {
            int samples_returned = inlet.pull_chunk(data_buffer, timestamp_buffer);
         
            if (samples_returned > 0)
            {
                // There are many things you can do with the incoming chunk to make it more palatable for Unity.
                // Note that if you are going to do significant processing and feature extraction on your signal,
                // it makes much more sense to do that in an external process then have that process output its
                // result to yet another stream that you capture in Unity.
                // Most of the time we only care about the latest sample to get a visual representation of the latest
                // state, so that's what we do here: take the last sample only and use it to udpate the object scale.
                float x = data_buffer[samples_returned - 1, 0];
                
                float y = data_buffer[samples_returned - 1, 1];
                float z = data_buffer[samples_returned - 1, 2];

                Debug.Log($"{x}, {y}, {z}");
                var new_scale = new Vector3(x, y, z);
               
                gameObject.transform.localScale = new_scale;
            }
        }
    }
}


