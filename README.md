# DupsBegone
Yet another file/folder de-duplicator in C#, multi-platform via mono.

Differentiating features:
 1. Prioritises finding duplicate folders over duplicate files.
 2. Optimised UI for manual de-dup display and decisions.
 3. Optimised algorithm to display some dups as fast as possible, while scanning continues in the background.
    (Essentially it shallow-scans to find potential duplicates before it deeps scans)
 4. Totally ignores folder and file names. Only file content is used for duplicate detection. (Maybe not very unique).
 5. Multi-platform.
