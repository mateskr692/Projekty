#pragma once

void count_chars_in_file(FILE *input, struct list **headp);
void encode_tree(struct tree *root, struct buffer *b);
void encode_file(FILE *input, struct buffer *b, struct translation *table);
struct tree *decode_tree(struct tree *root, struct buffer *b);
void decode_file(struct tree *root, struct buffer *b, FILE *output);

void demo_encode(FILE *input, FILE *output);
void demo_decode(FILE *input, FILE *output);