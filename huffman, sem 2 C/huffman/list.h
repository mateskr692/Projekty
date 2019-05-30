#pragma once

//the list is build up by counting characters from input file
//At first it conatins single branch trees which then are connected by their cunt
//This reduces the list to a single "root" element at end
struct list
{
	struct list *next;
	struct tree *subtree;
};

void list_add_char(struct list **headp, uint8_t ch);
struct list *list_remove_lowest(struct list **headp);
void list_print(struct list *head);