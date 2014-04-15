
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;"
ASSEMBLY = bin/Terradue.OpenSearch.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin

TERRADUE_OPENSEARCH_DLL_MDB_SOURCE=bin/Terradue.OpenSearch.dll.mdb
TERRADUE_OPENSEARCH_DLL_MDB=$(BUILD_DIR)/Terradue.OpenSearch.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Terradue.OpenSearch.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin

TERRADUE_OPENSEARCH_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(TERRADUE_OPENSEARCH_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(TERRADUE_OPENSEARCH_PC)  


RESGEN=resgen2

TERRADUE_OPENSEARCH_PC = $(BUILD_DIR)/terradue.opensearch.pc

FILES = \
	Properties/AssemblyInfo.cs \
	Terradue/OpenSearch/Engine/Extensions/GeoSpatialTemporalOpenSearchExtension.cs \
	Terradue/OpenSearch/Engine/Extensions/RdfOpenSearchEngineExtension.cs \
	Terradue/OpenSearch/Engine/IOpenSearchEngineExtension.cs \
	Terradue/OpenSearch/Engine/OpenSearchEngine.cs \
	Terradue/OpenSearch/Engine/OpenSearchEngineExtension.cs \
	Terradue/OpenSearch/Exception/ImpossibleSearchException.cs \
	Terradue/OpenSearch/Filters/OpenSearchCorrelationFilter.cs \
	Terradue/OpenSearch/Filters/OpenSearchMemoryCache.cs \
	Terradue/OpenSearch/OpenSearchable/GenericOpenSearchable.cs \
	Terradue/OpenSearch/OpenSearchable/IOpenSearchable.cs \
	Terradue/OpenSearch/OpenSearchable/IOpenSearchableFactory.cs \
	Terradue/OpenSearch/OpenSearchable/IProxiedOpenSearchable.cs \
	Terradue/OpenSearch/OpenSearchable/MultiGenericOpenSearchable.cs \
	Terradue/OpenSearch/Request/FileOpenSearchRequest.cs \
	Terradue/OpenSearch/Request/HttpOpenSearchRequest.cs \
	Terradue/OpenSearch/Request/MemoryOpenSearchRequest.cs \
	Terradue/OpenSearch/Request/MultiAtomOpenSearchRequest.cs \
	Terradue/OpenSearch/Request/OpenSearchRequest.cs \
	Terradue/OpenSearch/Response/AtomOpenSearchResponse.cs \
	Terradue/OpenSearch/Response/HttpOpenSearchResponse.cs \
	Terradue/OpenSearch/Response/MemoryOpenSearchResponse.cs \
	Terradue/OpenSearch/Response/OpenSearchResponse.cs \
	Terradue/OpenSearch/Result/IOpenSearchResult.cs \
	Terradue/OpenSearch/Result/IOpenSearchResultCollection.cs \
	Terradue/OpenSearch/Result/OpenSearchResult.cs \
	Terradue/OpenSearch/Result/RdfXmlDocument.cs \
	Terradue/OpenSearch/Schema/OpenSearchDescription.cs \
	Terradue/OpenSearch/OpenSearchFactory.cs \
	Terradue/OpenSearch/OpenSearchUrl.cs \
	Terradue/OpenSearch/Schema/UniqueValueDictionary.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	terradue.opensearch.pc.in 

REFERENCES =  \
	System \
	-pkg:mono-addins \
	System.Xml \
	System.Net \
	System.Core \
	System.ServiceModel \
	System.Runtime.Caching \
	System.Runtime.Serialization \
	System.Web

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-wrapper,TERRADUE_OPENSEARCH_PC,terradue.opensearch.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'


$(ASSEMBLY_MDB): $(ASSEMBLY)
$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(shell dirname $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-install-local-hook prefix=$(prefix)
	make install-satellite-assemblies prefix=$(prefix)
	mkdir -p '$(DESTDIR)$(libdir)/$(PACKAGE)'
	$(call cp,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_OPENSEARCH_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)/pkgconfig'
	$(call cp,$(TERRADUE_OPENSEARCH_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_OPENSEARCH_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_OPENSEARCH_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
