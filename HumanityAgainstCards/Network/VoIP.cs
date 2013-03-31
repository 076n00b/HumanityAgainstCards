using ManateesAgainstCards.Network.Packets;
using SFML.Audio;
using NSpeex;

namespace ManateesAgainstCards.Network
{
	class VoIp : SoundRecorder
	{
		protected override bool OnProcessSamples(short[ ] samples)
		{
			SpeexEncoder encoder = new SpeexEncoder(BandMode.Wide);

			for (int i = 0; i < samples.Length; i += encoder.FrameSize)
			{
				byte[] outData = new byte[encoder.FrameSize];
				if (encoder.Encode(samples, i, i - encoder.FrameSize > encoder.FrameSize ? encoder.FrameSize : i - encoder.FrameSize, outData, 0, outData.Length) != 0)
					Client.SendMessage(new Voice(outData));
			}

			return true;
		}
	}
}
