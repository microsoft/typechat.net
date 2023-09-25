﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A collection of named vocabularies.
/// Vocabulary collections can be dynamic and load vocabularies at runtime from data bases, files and so on
/// </summary>
public interface IVocabCollection : IEnumerable<NamedVocab>
{
    NamedVocab? Get(string name);
}

