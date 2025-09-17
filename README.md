# Midify
My own Midi application!

## usage

From the command line enter: Midifi <command> <in:midifilename> <out:textfilename>
Available commands are:
- summary: gives a summary of the midi file (header, list of tracks with their instruments and number of events)
- details: gives a detailed list of all events per track in the midi file

## To do list
- nicer view of details output (it's still a bit messy)
- meta events time and key signature displayed as it would be in music (sort of)
- midi event position displayed as measure, beat and tick
- ability to play a midi file from the command line
- ability to filter channels/tracks when playing a midi file
- ability to select a time range when playing a midi file
- generate an encoded message as a midi track
- decode an encoded message from a midi track
- initiate the compose module with a simple add track command (track name and instrument)
- compose module: being able to enter notes in the command line
- compose module: program a sequence of notes
- compose module: repeat a sequuence, scaling or follow chord structure
- compose module: export everything to midi file
- compose module: playback all
- compose module: playback a selection of tracks