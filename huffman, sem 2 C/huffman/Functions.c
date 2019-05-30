#include "stdafx.h"

void count_chars_in_file(FILE *input, struct list **headp)
{
	//count characters in file and add them to list
	int8_t ch = 0;
	while ((ch = getc(input)) != EOF)
		list_add_char(headp, ch);
	//add 1 pseudo eof character to list
	list_add_char(headp, PSEUDO_EOF);

	//reset position to begining
	fseek(input, 0, SEEK_SET);

	return;
}

void encode_tree(struct tree *root, struct buffer *b)
//if tree node has children it sends 0 + its left and right children, if not, sends 1 + character code
{
	if (root->left != NULL)
	{
		buffer_send(b, 0, 1); //a single "0" bit
		encode_tree(root->left, b);
		encode_tree(root->right, b);
		return;
	}
	else
	{
		buffer_send(b, 128, 1); //a single "1" bit
		buffer_send(b, root->ch, 8);
		return;
	}
}

void encode_file(FILE *input, struct buffer *b, struct translation *table)
{
	int8_t ch = 0;
	uint8_t endflag = 0;
	while (endflag == 0)
	{
		//send pseudo eof character after reaching end of file
		if ((ch = getc(input)) == EOF)
		{
			ch = PSEUDO_EOF;
			endflag = 1;
		}

		struct translation *entry = translation_find_char(table, ch);
		uint8_t bs = entry->bitsize;
		for (int i = 0; i < (entry->bitsize - 1) / 8 + 1; i++)
		{
			if (bs >= 8)
			{
				buffer_send(b, entry->bittable[i], 8);
				bs -= 8;
			}
			else
				buffer_send(b, entry->bittable[i], bs);
		}
	}
	//save remaining bits
	putc(b->byte, b->f);
}

struct tree *decode_tree(struct tree *root, struct buffer *b)
{
	uint8_t bit = buffer_load_1bit(b);
	if (root == NULL)
	{
		root = (struct tree*)malloc(sizeof(struct tree));
		root->left = NULL;
		root->right = NULL;
	}
	if (bit == 0)
	{
		root->left = decode_tree(root->left, b);
		root->right = decode_tree(root->right, b);
	}
	else
		root->ch = buffer_load_8bit(b);

	return root;
}

void decode_file(struct tree *root, struct buffer *b, FILE *output)
{
	while(1)
	{
		struct tree *aux = root;
		while (aux->left != NULL)
		{
			uint8_t bit = buffer_load_1bit(b);
			if (bit == 0)
				aux = aux->left;
			else
				aux = aux->right;
		}
		uint8_t ch = aux->ch;
		if (ch == PSEUDO_EOF)
			return;
		putc(ch, output);
	}
}

void demo_encode(FILE *input, FILE *output)
{
	struct list *head = NULL;
	struct tree *root = NULL;
	struct translation *table = NULL;
	struct buffer *b = buffer_initiate(output);
	count_chars_in_file(input, &head);
	tree_build_from_list(&head, &root);
	translation_build_from_tree(&table, root, 0);
	
	encode_tree(root, b);
	encode_file(input, b, table);

	free(b);
	tree_delete(root);
	translation_delete(&table);
	fclose(input);
	fclose(output);
}
void demo_decode(FILE *input, FILE *output)
{
	struct tree *root = NULL;
	struct buffer *b = buffer_initiate(input);
	root = decode_tree(root, b);

	decode_file(root, b, output);

	free(b);
	tree_delete(root);
	fclose(input);
	fclose(output);
}